<script setup lang="ts">
import { ref, onMounted, watch } from 'vue';
import { useRouter } from 'vue-router';
import FightReviewer from '../components/combat/FightReviewer.vue';
import { postFight, getBattleList, getBattleReplay } from '../api/battle';
import type { BattleRecordDto } from '../api/battle';
import type { BattleEvent } from '../types/battleEvents';

// ── Props（由 router state 注入，PK 对局用）────────────────
const props = defineProps<{
  battleInitData?: {
    attackerId: string;
    defenderId: string;
    timestamp?: number;
  };
}>();

const router = useRouter();

// ── 战斗状态 ─────────────────────────────────────────────
const battleEvents  = ref<BattleEvent[] | null>(null);
const isFighting    = ref(false);
const isDataError   = ref(false);

// ── 历史列表状态 ──────────────────────────────────────────
const historyList      = ref<BattleRecordDto[]>([]);
const isHistoryLoading = ref(false);
const selectedRecord   = ref<BattleRecordDto | null>(null);
const isReplayLoading  = ref(false);

// ── 日期格式化 ────────────────────────────────────────────
function formatDate(iso: string): string {
  const d = new Date(iso);
  const pad = (n: number) => String(n).padStart(2, '0');
  return `${d.getFullYear()}-${pad(d.getMonth() + 1)}-${pad(d.getDate())} ${pad(d.getHours())}:${pad(d.getMinutes())}`;
}

// ── 加载历史列表 ──────────────────────────────────────────
async function loadHistory() {
  isHistoryLoading.value = true;
  try {
    historyList.value = await getBattleList();
  } catch (e) {
    console.error('加载历史失败', e);
  } finally {
    isHistoryLoading.value = false;
  }
}

// ── 选中一条记录（toggle） ────────────────────────────────
function selectRecord(record: BattleRecordDto) {
  selectedRecord.value =
    selectedRecord.value?.id === record.id ? null : record;
}

// ── 播放选中的回放 ────────────────────────────────────────
async function playReplay() {
  if (!selectedRecord.value || isReplayLoading.value) return;
  isReplayLoading.value = true;
  isDataError.value     = false;
  battleEvents.value    = null;
  try {
    battleEvents.value = await getBattleReplay(selectedRecord.value.id);
  } catch (e) {
    console.error('加载回放失败', e);
    isDataError.value = true;
  } finally {
    isReplayLoading.value = false;
  }
}

// ── 执行 live 对局（PK 按钮来的）────────────────────────
async function runLiveFight() {
  if (!props.battleInitData) return;
  isFighting.value   = true;
  isDataError.value  = false;
  battleEvents.value = null;
  try {
    const res = await postFight(
      props.battleInitData.attackerId,
      props.battleInitData.defenderId,
      undefined,
    );
    if (res) {
      battleEvents.value = res;
    } else {
      isDataError.value = true;
    }
  } catch (e) {
    console.error('战斗失败', e);
    isDataError.value = true;
  } finally {
    isFighting.value = false;
  }
}

// ── 关闭 FightReviewer ────────────────────────────────────
function handleClose() {
  battleEvents.value  = null;
  selectedRecord.value = null;
}

// ── 生命周期 ──────────────────────────────────────────────
onMounted(async () => {
  await loadHistory();
  if (props.battleInitData) {
    runLiveFight();
  }
});

watch(() => props.battleInitData, (data) => {
  if (data) runLiveFight();
});
</script>

<template>
  <div class="flex flex-col h-screen bg-slate-950 text-slate-200 overflow-hidden">

    <!-- ── Header ───────────────────────────────────────────── -->
    <header class="h-16 border-b border-white/10 flex items-center justify-center
                   bg-slate-900/50 backdrop-blur-md relative shrink-0">
      <h1 class="text-2xl font-black italic tracking-widest text-transparent
                 bg-clip-text bg-linear-to-r from-blue-400 to-indigo-500">
        Battle Center
      </h1>
      <div class="absolute bottom-0 w-full h-px
                  bg-linear-to-r from-transparent via-blue-500 to-transparent opacity-40" />
    </header>

    <main class="flex flex-1 overflow-hidden">

      <!-- ── Arena ────────────────────────────────────────────── -->
      <section class="flex-1 relative flex items-center justify-center p-6
                      bg-[radial-gradient(circle_at_50%_40%,rgba(30,41,59,0.6),transparent)]">

        <!-- 加载中（live fight） -->
        <div v-if="isFighting || isReplayLoading"
             class="flex flex-col items-center gap-4">
          <div class="w-10 h-10 border-2 border-blue-500/20 border-t-blue-500
                      rounded-full animate-spin" />
          <p class="text-blue-400 font-mono text-sm tracking-widest animate-pulse">
            {{ isFighting ? 'SIMULATING BATTLE...' : 'LOADING REPLAY...' }}
          </p>
        </div>

        <!-- 播放器 -->
        <FightReviewer
          v-else-if="battleEvents"
          class="w-full h-full"
          :battleData="battleEvents"
          @close="handleClose"
        />

        <!-- 错误 -->
        <div v-else-if="isDataError"
             class="flex flex-col items-center gap-4 text-rose-500/60">
          <span class="text-4xl">🚫</span>
          <p class="font-bold tracking-widest">无法加载对局数据</p>
          <button @click="isDataError = false"
                  class="text-xs text-slate-500 hover:text-white transition-colors mt-2">
            关闭
          </button>
        </div>

        <!-- 空状态：选择历史回放 -->
        <div v-else class="flex flex-col items-center gap-5 opacity-30 select-none">
          <span class="text-7xl">⚔️</span>
          <p class="text-lg font-bold tracking-widest">从右侧选择对局回放</p>
        </div>
      </section>

      <!-- ── 右侧历史面板 ──────────────────────────────────── -->
      <aside class="w-80 border-l border-white/10 bg-black/30 flex flex-col shrink-0">

        <!-- 返回按钮 -->
        <div class="p-4 border-b border-white/5 shrink-0">
          <button @click="router.push('/lobby')"
                  class="w-full py-2.5 bg-white/5 hover:bg-white/10 border border-white/10
                         rounded-xl transition-all flex items-center justify-center gap-2 group text-sm">
            <span class="group-hover:-translate-x-1 transition-transform">↩</span>
            返回大厅
          </button>
        </div>

        <!-- 列表标题 -->
        <div class="px-4 pt-4 pb-2 shrink-0 flex items-center justify-between">
          <h3 class="text-[10px] font-bold text-slate-500 uppercase tracking-widest">
            历史对局
          </h3>
          <span class="text-[10px] text-slate-700 font-mono">
            {{ historyList.length }} 场
          </span>
        </div>

        <!-- 列表内容 -->
        <div class="flex-1 overflow-y-auto px-3 pb-4 space-y-2 custom-scrollbar">

          <!-- 加载中 -->
          <div v-if="isHistoryLoading"
               class="flex items-center justify-center py-12 opacity-40">
            <div class="w-5 h-5 border-2 border-slate-500 border-t-transparent
                        rounded-full animate-spin" />
          </div>

          <!-- 空列表 -->
          <div v-else-if="historyList.length === 0"
               class="flex flex-col items-center gap-2 py-12 text-center opacity-30">
            <span class="text-3xl">📭</span>
            <p class="text-xs text-slate-500">还没有对局记录</p>
          </div>

          <!-- 历史条目 -->
          <div v-else
               v-for="record in historyList"
               :key="record.id"
               @click="selectRecord(record)"
               class="rounded-xl border transition-all cursor-pointer overflow-hidden"
               :class="selectedRecord?.id === record.id
                 ? 'border-blue-500/50 bg-blue-500/8'
                 : 'border-white/5 bg-white/3 hover:border-white/15 hover:bg-white/5'">

            <!-- 条目主体 -->
            <div class="p-3 flex items-center gap-3">

              <!-- 胜负徽章 -->
              <div class="shrink-0 w-9 h-9 rounded-lg flex items-center justify-center
                          font-black text-[11px] uppercase tracking-wider"
                   :class="record.isWin
                     ? 'bg-emerald-900/60 text-emerald-300 border border-emerald-500/30'
                     : 'bg-rose-900/60 text-rose-300 border border-rose-500/30'">
                {{ record.isWin ? 'WIN' : 'LOSE' }}
              </div>

              <!-- 信息 -->
              <div class="flex-1 min-w-0">
                <p class="text-sm font-bold text-white truncate">
                  vs {{ record.opponentName }}
                </p>
                <p class="text-[10px] text-slate-600 font-mono mt-0.5">
                  {{ formatDate(record.createdTime) }}
                </p>
              </div>

              <!-- 选中箭头 -->
              <div v-if="selectedRecord?.id === record.id"
                   class="text-blue-400 text-xs shrink-0">▶</div>
            </div>

            <!-- 播放按钮（选中展开） -->
            <div v-if="selectedRecord?.id === record.id"
                 class="px-3 pb-3">
              <button
                @click.stop="playReplay"
                :disabled="isReplayLoading"
                class="w-full py-2 rounded-lg bg-blue-600 hover:bg-blue-500
                       text-white text-xs font-black uppercase tracking-widest
                       transition-all disabled:opacity-40 disabled:cursor-not-allowed
                       shadow-[0_0_16px_rgba(59,130,246,0.3)]">
                <span v-if="isReplayLoading">加载中…</span>
                <span v-else>▶ 播放回放</span>
              </button>
            </div>
          </div>

        </div>
      </aside>
    </main>
  </div>
</template>

<style scoped>
.custom-scrollbar::-webkit-scrollbar       { width: 4px; }
.custom-scrollbar::-webkit-scrollbar-track { background: transparent; }
.custom-scrollbar::-webkit-scrollbar-thumb { background: rgba(255,255,255,0.08); border-radius: 10px; }
</style>
