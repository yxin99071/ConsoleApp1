<template>
  <div class="battle-stage" :class="{ 'battle-over': isFinished }">
    <div class="column side-column">
      <div v-if="p1" class="player-card" 
           :class="{ 
             'is-winner': isFinished && winnerName === p1.Name,
             'is-loser': isFinished && winnerName !== p1.Name,
             'clinging-to-life': p1.CurrentHP <= 0 && !isFinished 
           }">
        <div class="card-section info">
          <div class="name-row">
            <span class="status-icon" v-if="p1.CurrentHP <= 0 && !isFinished">🛡️ 不屈</span>
            <span class="status-icon" v-if="isFinished && winnerName === p1.Name">🏆 胜出</span>
            <span class="name">{{ p1.Name }}</span>
          </div>
          <div class="hp-wrapper">
            <div class="hp-bar" :class="{ 'critical': p1.CurrentHP <= 0 }"
                 :style="{ width: Math.max(0, (p1.CurrentHP / p1.Stats.MaxHealth * 100)) + '%' }"></div>
            <span class="hp-text">{{ p1.CurrentHP }} / {{ p1.Stats.MaxHealth }}</span>
          </div>
        </div>
        
        <div class="card-section">
          <div class="label">武器</div>
          <div class="item-list">
            <span v-for="w in p1.Weapons" :key="w.Name" class="badge weapon">{{ w.Name }}</span>
          </div>
        </div>

        <div class="card-section">
          <div class="label">技能</div>
          <div class="item-list">
            <span v-for="s in p1.Skills" :key="s.Name" class="badge skill">{{ s.Name }}</span>
          </div>
        </div>

        <div class="card-section buffs">
          <div class="label">状态</div>
          <div class="item-list">
            <transition-group name="buff-anim">
              <span v-for="b in p1.ActiveBuffs" :key="b.name" class="badge buff">
                {{ b.name }} <small>Lv.{{ b.level }}</small>
                <span class="round-tick" v-if="b.rounds">⏳{{ b.rounds }}</span>
              </span>
            </transition-group>
          </div>
        </div>
      </div>
    </div>

    <div class="column center-column">
      <div class="log-container">
        <div class="log-header">
          <span>BATTLE LOG</span>
          <input type="range" v-model="playDelay" min="100" max="1500" title="调整播放速度">
        </div>
        
        <div class="log-content" ref="logRef">
          <div v-for="(log, i) in displayLogs" :key="i" 
               class="log-row"
               :style="{ paddingLeft: (log.depth * 20) + 'px' }">
            <span v-if="log.depth > 0" class="depth-marker">↪</span>
            
            <span :class="['log-text', log.type]">
              {{ log.msg }}
            </span>
          </div>
        </div>
        
        <div class="controls">
          <button class="btn replay" @click="startReplay" :disabled="isPlaying">
            {{ isPlaying ? '回放中...' : '开始回放' }}
          </button>
        </div>
      </div>
    </div>

    <div class="column side-column">
      <div v-if="p2" class="player-card" 
           :class="{ 
             'is-winner': isFinished && winnerName === p2.Name,
             'is-loser': isFinished && winnerName !== p2.Name,
             'clinging-to-life': p2.CurrentHP <= 0 && !isFinished 
           }">
           <div class="card-section info">
          <div class="name-row">
            <span class="status-icon" v-if="p2.CurrentHP <= 0 && !isFinished">🛡️ 不屈</span>
            <span class="status-icon" v-if="isFinished && winnerName === p2.Name">🏆 胜出</span>
            <span class="name">{{ p2.Name }}</span>
          </div>
          <div class="hp-wrapper">
            <div class="hp-bar" :class="{ 'critical': p2.CurrentHP <= 0 }"
                 :style="{ width: Math.max(0, (p2.CurrentHP / p2.Stats.MaxHealth * 100)) + '%' }"></div>
            <span class="hp-text">{{ p2.CurrentHP }} / {{ p2.Stats.MaxHealth }}</span>
          </div>
        </div>
        <div class="card-section"><div class="label">武器</div><div class="item-list"><span v-for="w in p2.Weapons" :key="w.Name" class="badge weapon">{{ w.Name }}</span></div></div>
        <div class="card-section"><div class="label">技能</div><div class="item-list"><span v-for="s in p2.Skills" :key="s.Name" class="badge skill">{{ s.Name }}</span></div></div>
        <div class="card-section buffs">
          <div class="label">状态</div>
          <div class="item-list">
            <transition-group name="buff-anim">
              <span v-for="b in p2.ActiveBuffs" :key="b.name" class="badge buff">{{ b.name }} <small>Lv.{{ b.level }}</small><span class="round-tick" v-if="b.rounds">⏳{{ b.rounds }}</span></span>
            </transition-group>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, nextTick } from 'vue';
import type { BattleEvent, Player } from '@/Models/BattleInterface';

const props = defineProps<{ rawJson: BattleEvent[] }>();

const p1 = ref<Player | null>(null);
const p2 = ref<Player | null>(null);
// 增加 depth 字段
const displayLogs = ref<{ type: string; msg: string; depth: number }[]>([]);
const isPlaying = ref(false);
const isFinished = ref(false);
const winnerName = ref<string | null>(null);
const logRef = ref<HTMLElement | null>(null);
const playDelay = ref(600);

const sleep = (ms: number) => new Promise(res => setTimeout(res, ms));

const startReplay = async () => {
  if (isPlaying.value) return;
  isPlaying.value = true;
  isFinished.value = false;
  winnerName.value = null;
  displayLogs.value = [];

  // 安全检查
  if (!props.rawJson || props.rawJson.length === 0) {
    displayLogs.value.push({ type: 'sys', msg: '没有战斗数据', depth: 0 });
    isPlaying.value = false;
    return;
  }

  // --- 播放循环 ---
  for (const event of props.rawJson) {
    await processEvent(event);
    await sleep(playDelay.value);
    await nextTick();
    if (logRef.value) logRef.value.scrollTop = logRef.value.scrollHeight;
  }

  // --- 新增：自动推断胜负逻辑 (兜底后端没发 BattleEnd 的情况) ---
  if (!isFinished.value && p1.value && p2.value) {
    // 情况 A: P1 死了，P2 活着 -> P2 胜
    if (p1.value.CurrentHP <= 0 && p2.value.CurrentHP > 0) {
      isFinished.value = true;
      winnerName.value = p2.value.Name;
      displayLogs.value.push({ type: 'sys', msg: '⚠️ (本地判定) 战斗结束，胜者判定为：' + p2.value.Name, depth: 0 });
    }
    // 情况 B: P2 死了，P1 活着 -> P1 胜
    else if (p2.value.CurrentHP <= 0 && p1.value.CurrentHP > 0) {
      isFinished.value = true;
      winnerName.value = p1.value.Name;
      displayLogs.value.push({ type: 'sys', msg: '⚠️ (本地判定) 战斗结束，胜者判定为：' + p1.value.Name, depth: 0 });
    }
    // 情况 C: 同归于尽
    else if (p1.value.CurrentHP <= 0 && p2.value.CurrentHP <= 0) {
      isFinished.value = true;
      displayLogs.value.push({ type: 'sys', msg: '⚠️ (本地判定) 战斗结束，平局', depth: 0 });
    }
    // 情况 D: 都还活着 (可能是日志不完整)
    else {
      displayLogs.value.push({ type: 'sys', msg: '❓ 数据流播放完毕，但无人倒下。', depth: 0 });
    }
  }
  
  isPlaying.value = false;
};

const processEvent = async (event: BattleEvent) => {
  const { Type, Data, Depth } = event; // 解构出 Depth
  const d = Depth || 0; // 默认深度为 0

  const findP = (name: string) => [p1.value, p2.value].find(p => p?.Name === name);

  switch (Type) {
    case 'BattleStart':
      p1.value = { ...Data.Players[0], CurrentHP: Data.Players[0].Stats.MaxHealth, ActiveBuffs: [] };
      p2.value = { ...Data.Players[1], CurrentHP: Data.Players[1].Stats.MaxHealth, ActiveBuffs: [] };
      displayLogs.value.push({ type: 'sys', msg: '--- 战斗开始 ---', depth: 0 });
      break;

    case 'Action':
      displayLogs.value.push({ type: 'act', msg: `▶️ ${Data.Actor} 发动了 ${Data.Name}`, depth: d });
      break;

    case 'Damage':
      const target = findP(Data.Target);
      if (target) target.CurrentHP = Data.HP;
      displayLogs.value.push({ type: 'dmg', msg: `💥 ${Data.Target} 受到 ${Data.Value} 伤害 (剩: ${Data.HP})`, depth: d });
      break;

    case 'BuffApply':
      const bTarget = findP(Data.Target);
      if (bTarget) {
        const idx = bTarget.ActiveBuffs.findIndex(b => b.name === Data.BuffName);
        const bData = { name: Data.BuffName, level: Data.BuffLevel, rounds: Data.LastRound };
        if (idx > -1) bTarget.ActiveBuffs[idx] = bData;
        else bTarget.ActiveBuffs.push(bData);
      }
      displayLogs.value.push({ type: 'buff', msg: `✨ ${Data.Target} 获得 ${Data.BuffName}`, depth: d });
      break;

    case 'BuffTimeOut':
      const toTarget = findP(Data.Unit);
      if (toTarget) toTarget.ActiveBuffs = toTarget.ActiveBuffs.filter(b => b.name !== Data.BuffName);
      displayLogs.value.push({ type: 'sys', msg: `⌛ ${Data.Unit} 的 ${Data.BuffName} 效果消失`, depth: d });
      break;

    case 'BuffTick':
      displayLogs.value.push({ type: 'buff', msg: `💢 ${Data.Unit} 承受 ${Data.BuffName} 伤害 -${Data.Damage}`, depth: d });
      break;

    case 'Passive':
      displayLogs.value.push({ type: 'pass', msg: `⚡ ${Data.Unit} 触发被动: ${Data.SkillName}`, depth: d });
      break;

    case 'BattleEnd':
      isFinished.value = true;
      winnerName.value = Data.UserName;
      displayLogs.value.push({ type: 'sys', msg: `🏁 战斗结束！胜者: ${Data.UserName}`, depth: 0 });
      break;
  }
};
</script>

<style scoped>
/* 1. 布局修复：全屏固定，内部滚动 */
.battle-stage {
  display: flex;
  justify-content: center;
  gap: 10px;
  background: #0d1117;
  height: 80vh; /* 强制占满视口高度 */
  overflow: hidden; /* 禁止整个页面滚动 */
  padding: 20px;
  box-sizing: border-box;
}

.column {
  height: 100%;
  display: flex;
  flex-direction: column;
}

.side-column {
  flex: 0 0 280px; /* 固定卡片宽度 */
  overflow-y: auto; /* 卡片太长可以自己滚，但不影响布局 */
}

.center-column {
  flex: 1; /* 占据剩余空间 */
  max-width: 600px;
  min-width: 300px;
}

/* 2. 角色卡片样式 */
.player-card {
  background: #161b22;
  border: 1px solid #30363d;
  border-radius: 12px;
  padding: 15px;
  transition: all 0.5s ease;
  margin-bottom: 20px;
}
.card-section { margin-bottom: 12px; padding: 8px; background: #010409; border-radius: 6px; }

/* 状态特效 */
.clinging-to-life { border-color: #f85149; box-shadow: 0 0 15px rgba(248, 81, 73, 0.3); animation: pulse 2s infinite; }
.is-winner { border-color: #f2cc60; box-shadow: 0 0 20px rgba(242, 204, 96, 0.4); transform: scale(1.02); }
.is-loser { filter: grayscale(1); opacity: 0.5; }

/* 血条 */
.hp-wrapper { height: 18px; background: #30363d; border-radius: 9px; overflow: hidden; position: relative; margin-top:5px;}
.hp-bar { height: 100%; background: #238636; transition: width 0.3s ease; }
.hp-bar.critical { background: #f85149; }
.hp-text { position: absolute; width: 100%; text-align: center; font-size: 10px; color: white; line-height: 18px; font-weight: bold; }

/* 3. 日志容器修复：Flex撑开，滚动条在内部 */
.log-container {
  height: 100%; /* 撑满 center-column */
  display: flex;
  flex-direction: column;
  background: #010409;
  border: 1px solid #30363d;
  border-radius: 12px;
  overflow: hidden; /* 关键：防止子元素溢出 */
}

.log-header {
  padding: 10px 15px;
  background: #161b22;
  border-bottom: 1px solid #30363d;
  color: #8b949e;
  font-size: 12px;
  display: flex; justify-content: space-between; align-items: center;
  flex-shrink: 0; /* 防止头被压扁 */
}

/* 滚动区域核心 */
.log-content {
  flex: 1; /* 自动占据剩余高度 */
  overflow-y: auto; /* 只有这里出现滚动条 */
  padding: 15px;
  font-family: 'Consolas', monospace;
  font-size: 13px;
  line-height: 1.6;
}

/* 4. 日志条目与 Depth 表现 */
.log-row {
  display: flex;
  align-items: flex-start;
  margin-bottom: 6px;
  transition: all 0.2s;
}

/* 修改这一段 css */
.depth-marker {
  color: #e3b341; /* 金色箭头 */
  margin-right: 5px;
  font-weight: bold;
  font-family: monospace;
}

.log-text { word-break: break-all; }
.dmg { color: #f85149; }
.act { color: #58a6ff; }
.buff { color: #d2a8ff; }
.pass { color: #e3b341; }
.sys { color: #8b949e; font-style: italic; }

/* 底部按钮 */
.controls {
  padding: 15px;
  border-top: 1px solid #30363d;
  background: #161b22;
  text-align: center;
  flex-shrink: 0;
}
.btn.replay {
  padding: 8px 20px; background: #238636; border: none; color: white; border-radius: 6px; cursor: pointer; font-weight: bold;
}
.btn.replay:disabled { background: #30363d; color: #8b949e; cursor: not-allowed; }

/* Badge 样式 */
.badge { font-size: 10px; padding: 2px 6px; border-radius: 4px; margin: 2px; display: inline-block; background: #21262d; border: 1px solid #30363d; color: #c9d1d9; }
.weapon { border-color: #8e44ad; color: #d2a8ff; }
.skill { border-color: #2980b9; color: #58a6ff; }

/* 滚动条美化 (Chrome/Safari) */
::-webkit-scrollbar { width: 8px; }
::-webkit-scrollbar-track { background: #010409; }
::-webkit-scrollbar-thumb { background: #30363d; border-radius: 4px; }
::-webkit-scrollbar-thumb:hover { background: #484f58; }

@keyframes pulse { 0% { opacity: 1; } 50% { opacity: 0.7; } 100% { opacity: 1; } }
</style>