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
  <Transition name="fade">
  <div v-if="isFinished && settlementData" class="settlement-overlay">
    <div class="settlement-card">
      <h2>战斗结算</h2>
      <div class="winner-tag">胜者: {{ winnerName }}</div>
      
      <div class="exp-bar-container">
         <p>经验获得: +{{ settlementData.ExperienceChange.Gained }}</p>
         </div>

      <div v-if="settlementData.LevelChange.IsLeveledUp" class="level-badge">
        LEVEL UP! {{ settlementData.LevelChange.To }}
      </div>

      <div class="stats-grid">
        <div v-for="(val, key) in settlementData.StatsChange" :key="key" class="stat-item">
          <span class="stat-label">{{ key }}</span>
          <span class="stat-old">{{ val.From }}</span>
          <span class="stat-arrow">➔</span>
          <span class="stat-new">{{ val.To }}</span>
        </div>
      </div>
      
      <button @click="isFinished = false">确认</button>
    </div>
  </div>
</Transition>
</template>

<script setup lang="ts">
import { ref, nextTick } from 'vue';
import type { BattleEvent, Player } from '@/Models/BattleInterface';
const settlementData = ref<any>(null);
const props = defineProps<{ rawJson: BattleEvent[] }>();

const p1 = ref<Player | null>(null);
const p2 = ref<Player | null>(null);
// 增加 depth 字段
const displayLogs = ref<{ 
  type: string; 
  msg: string; 
  depth: number; 
  isCrit?: boolean; // 新增：标识是否为暴击
}[]>([]);
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

      // 根据是否暴击选择图标
      const icon = Data.Critical ? '🔥 CRITICAL!' : '💥';
      const message = `${icon} ${Data.Target} 受到 ${Data.Value} 伤害 (剩: ${Data.HP})`;

      displayLogs.value.push({
        type: 'dmg',
        msg: message,
        depth: d,
        isCrit: Data.Critical // 将暴击状态存入 log 对象
      });
      break;

    case 'Dodge':
      displayLogs.value.push({
        type: 'dodge',
        msg: `🌬️ ${Data.Target} 灵活地闪避了攻击`,
        depth: d
      });
      break;

    case 'ReactionBegin':
      // 根据反应类型选择图标，如果是还击则用 ↩️
      const reactionIcon = Data.Type === 'Counter' ? '↩️' : '⚡';
      displayLogs.value.push({
        type: 'reaction',
        msg: `${reactionIcon} ${Data.Actor} 触发了 ${Data.Type}！`,
        depth: d
      });
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
      settlementData.value = Data; // 记录全量结算数据用于面板显示

      // A. 基础结束信息
      displayLogs.value.push({
        type: 'sys',
        msg: `🏁 战斗结束！最终胜者: ${Data.UserName}`,
        depth: 0
      });

      // B. 经验值奖励
      displayLogs.value.push({
        type: 'exp',
        msg: `📈 获得经验: +${Data.ExperienceChange.Gained} (当前: ${Data.ExperienceChange.After})`,
        depth: 0
      });

      // C. 等级提升检查
      if (Data.LevelChange.IsLeveledUp) {
        displayLogs.value.push({
          type: 'level-up',
          msg: `🎊 恭喜升级！Lv.${Data.LevelChange.From} ➔ Lv.${Data.LevelChange.To}`,
          depth: 0
        });

        // D. 属性成长详情 (如果升级了，遍历展示属性变化)
        const stats = Data.StatsChange;
        const statNames: Record<string, string> = {
          Health: '生命',
          Strength: '力量',
          Agility: '敏捷',
          Intelligence: '智力'
        };

        Object.entries(stats).forEach(([key, change]: [string, any]) => {
          const diff = change.To - change.From;
          if (diff !== 0) {
            displayLogs.value.push({
              type: 'stat-up',
              msg: `🔺 ${statNames[key] || key}: ${change.From} ➔ ${change.To} (${diff > 0 ? '+' : ''}${diff})`,
              depth: 0
            });
          }
        });
      }
      break;
    case 'Healing':
      const healerTarget = findP(Data.Target);
      if (healerTarget) {
        // 更新前端实体的生命值
        healerTarget.CurrentHP = Data.HP;
      }
      // 向日志列表推送治疗消息
      displayLogs.value.push({
        type: 'heal',
        msg: `💚 ${Data.Target} 恢复了 ${Data.Value} 点生命 (剩: ${Data.HP})`,
        depth: d,
      });
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

/* 升级日志特殊样式 */
.level-up {
  color: #f1c40f;
  font-weight: bold;
  background: rgba(241, 196, 15, 0.1);
  border-left: 4px solid #f1c40f;
  padding: 5px;
  margin: 5px 0;
}

/* 属性提升样式 */
.stat-up {
  color: #3498db;
  font-size: 0.9em;
  padding-left: 20px;
}

/* 经验值样式 */
.exp {
  color: #9b59b6;
}

/* 结算弹窗样式简述 */
.settlement-overlay {
  position: absolute;
  top: 0; left: 0; width: 100%; height: 100%;
  background: rgba(0,0,0,0.8);
  display: flex; justify-content: center; align-items: center;
  z-index: 100;
}
.settlement-card {
  background: #2c3e50;
  padding: 2rem;
  border-radius: 12px;
  border: 2px solid #3498db;
  text-align: center;
  min-width: 300px;
}
.stats-grid {
  display: grid;
  grid-template-columns: 1fr;
  gap: 10px;
  margin: 15px 0;
}
.stat-item {
  display: flex; justify-content: space-between;
  background: rgba(0,0,0,0.2);
  padding: 5px 10px;
  border-radius: 4px;
}
</style>