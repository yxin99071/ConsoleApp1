<template>
  <div class="init-page">
    <div class="init-container">
      <h2 class="title">角色觉醒</h2>

      <div class="form-section">
        <div class="form-row">
          <el-input v-model="form.name" placeholder="输入角色名">
            <template #prepend>名称</template>
          </el-input>
          <el-input v-model="form.account" placeholder="输入账号(选填)">
            <template #prepend>账号</template>
          </el-input>
        </div>
      </div>

      <div class="section-label">选择初始技能</div>
      <div class="skill-grid">
        <div 
          v-for="s in initialSkills" 
          :key="s.id" 
          :class="['skill-card', { active: isSkillSelected(s.id), disabled: form.mainJob === 'MORTAL' }]"
          @click="selectSkill(s.id)"
        >
          <div class="check-icon" v-if="isSkillSelected(s.id)">
            <el-icon><Check /></el-icon>
          </div>
          <div class="skill-name">{{ s.label }}</div>
          <div class="skill-desc">{{ s.desc }}</div>
        </div>
      </div>

      <div class="section-label">选择主职业</div>
      <div class="job-grid">
        <div 
          v-for="j in jobs" 
          :key="j.id" 
          :class="['job-card', { active: form.mainJob === j.id }]"
          @click="selectMain(j.id)"
        >
          <div class="job-icon">{{ j.icon }}</div>
          <span>{{ j.label }}</span>
        </div>
      </div>

      <div class="section-label" :class="{ disabled_text: form.mainJob === 'MORTAL' }">
        选择副职业 (可选)
      </div>
      <div class="job-grid" :class="{ 'is-disabled': form.mainJob === 'MORTAL' }">
        <div 
          v-for="j in subJobs" 
          :key="j.id" 
          :class="['job-card', 'sub', { active: form.subJob === j.id, locked: form.mainJob === j.id }]"
          @click="selectSub(j.id)"
        >
          <span>{{ j.label }}</span>
        </div>
      </div>

      <div class="info-section" v-if="form.mainJob">
        <div class="desc-box"><strong>职业特性：</strong>{{ currentJobDesc }}</div>
        <div class="attribute-info">
          <p v-for="(info, key) in attrUsage" :key="key">
            <span :style="{ color: statColors[key] }">● {{ statNames[key] }}:</span> {{ info }}
          </p>
        </div>
      </div>

      <div class="stats-display">
        <div v-for="(val, stat) in currentStats" :key="stat" class="stat-line">
          <span class="label">{{ statNames[stat] }}</span>
          <div class="bar-outer">
            <div 
              class="bar-inner" 
              :style="{ width: (val/60)*100 + '%', backgroundColor: statColors[stat] }"
            ></div>
          </div>
          <span class="val-text">{{ val }}</span>
        </div>
      </div>

      <div class="footer">
        <el-button 
          type="primary" 
          size="large" 
          :disabled="!canSubmit" 
          @click="submit"
        >
          开启冒险
        </el-button>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { reactive, computed } from 'vue';
import { useRouter } from 'vue-router';
import { Check } from '@element-plus/icons-vue';
import { ElMessage, ElMessageBox } from 'element-plus';
import { initProfileApi } from '@/api/user';

const router = useRouter();

// 1. 数据表定义
const initialSkills = [
  { id: 'FEIGN_DEATH', label: '假死', desc: 'HP低于0时回复至1点并净化全部负面效果。' },
  { id: 'WILL_OF_THE_DEAD', label: '亡者意志', desc: '濒死时依损血量获加成并反击(50%吸血)；若未击杀敌方或HP仍≤0则阵亡。' }
];

const jobs = [
  { id: 'WARRIOR', label: '战士', icon: '🛡️', primary: 'str', desc: '高力量与生命。暴击系数随等级快速增长，但暴击率增幅较低。' },
  { id: 'RANGER', label: '游侠', icon: '🏹', primary: 'agi', desc: '行动极快，闪避率高。暴击率随等级快速增长，但生命值较低。' },
  { id: 'MAGE', label: '法师', icon: '🪄', primary: 'int', desc: '通过智力修正伤害，不依赖暴击率。随等级提升伤害修正越高。' },
  { id: 'MORTAL', label: '凡人', icon: '👤', primary: 'all', desc: '全能发展，每级额外获1属性点。可全修互斥技能，但跨职技能效能略低。' }
];

const attrUsage: any = {
  str: '决定暴击伤害和生命。',
  agi: '决定暴击率和行动频率。',
  int: '影响伤害修正与少量生命。'
};

const statNames: any = { str: '力量', agi: '敏捷', int: '智力' };
const statColors: any = { str: '#f56c6c', agi: '#67c23a', int: '#409eff' };

// 2. 响应式表单
const form = reactive({
  name: '',
  account: '',
  mainJob: '',
  subJob: '',
  skillId: ''
});

// 3. 计算属性
const subJobs = computed(() => jobs.filter(j => j.id !== 'MORTAL'));
const currentJobDesc = computed(() => jobs.find(j => j.id === form.mainJob)?.desc || '');
const canSubmit = computed(() => form.name && form.mainJob && (form.mainJob === 'MORTAL' || form.skillId));

const currentStats = computed(() => {
  if (!form.mainJob || form.mainJob === 'MORTAL') return { str: 20, agi: 20, int: 20 };
  const stats = { str: 9, agi: 9, int: 9 };
  const mainP = jobs.find(j => j.id === form.mainJob)!.primary;
  if (!form.subJob) {
    stats[mainP as keyof typeof stats] = 42;
  } else {
    const subP = jobs.find(j => j.id === form.subJob)!.primary;
    stats[mainP as keyof typeof stats] = 36;
    stats[subP as keyof typeof stats] = 15;
  }
  return stats;
});

// 4. 交互函数
const isSkillSelected = (id: string) => {
  if (form.mainJob === 'MORTAL') return true;
  return form.skillId === id;
};

const selectSkill = (id: string) => {
  if (form.mainJob === 'MORTAL') return;
  form.skillId = id;
};

const selectMain = (id: string) => {
  form.mainJob = id;
  if (id === 'MORTAL' || form.subJob === id) form.subJob = '';
};

const selectSub = (id: string) => {
  if (form.mainJob === 'MORTAL' || form.mainJob === id) return;
  form.subJob = form.subJob === id ? '' : id;
};

const submit = async () => {
  try {
    await ElMessageBox.confirm('职业与初始技能一旦觉醒将无法更改，是否确认？', '命运确认', {
      confirmButtonText: '确定', cancelButtonText: '取消', type: 'warning'
    });
    
    const selectedSkills = form.mainJob === 'MORTAL' 
      ? ['FEIGN_DEATH', 'WILL_OF_THE_DEAD'] 
      : [form.skillId];

    await initProfileApi({
      name: form.name,
      account: form.account || undefined,
      profession: form.mainJob,
      secondProfession: form.subJob || null,
      initialSkills: selectedSkills
    } as any);

    ElMessage.success('觉醒成功！');
    router.replace({name:'UserHome'});
  } catch (e) {
    if (e !== 'cancel') ElMessage.error('初始化失败');
  }
};
</script>

<style scoped>
.init-page { min-height: 100vh; background: #0f0f0f; display: flex; justify-content: center; align-items: center; color: #eee; padding: 20px; }
.init-container { width: 550px; background: #1a1a1a; padding: 30px; border-radius: 16px; border: 1px solid #333; box-shadow: 0 20px 50px rgba(0,0,0,0.8); }

.title { text-align: center; margin-bottom: 25px; letter-spacing: 4px; color: #fff; }
.form-row { display: flex; gap: 15px; margin-bottom: 5px; }

.section-label { margin: 25px 0 12px; font-size: 13px; color: #666; font-weight: bold; border-left: 3px solid #444; padding-left: 10px; }
.section-label.disabled_text { text-decoration: line-through; opacity: 0.5; }

/* 技能卡片样式 */
.skill-grid { display: grid; grid-template-columns: 1fr 1fr; gap: 15px; }
.skill-card { 
  background: #222; border: 1px solid #333; padding: 15px; border-radius: 10px; 
  cursor: pointer; position: relative; transition: all 0.3s ease; 
}
.skill-card:hover { border-color: #555; }
.skill-card.active { border-color: #e6a23c; background: rgba(230, 162, 60, 0.08); box-shadow: 0 0 15px rgba(230, 162, 60, 0.2); }
.skill-card.disabled { cursor: default; }
.skill-name { font-weight: bold; color: #e6a23c; margin-bottom: 6px; font-size: 15px; }
.skill-desc { font-size: 11px; color: #888; line-height: 1.5; }
.check-icon { position: absolute; top: 8px; right: 10px; color: #e6a23c; font-size: 18px; }

/* 职业卡片样式 */
.job-grid { display: grid; grid-template-columns: repeat(4, 1fr); gap: 10px; }
.job-grid.is-disabled { opacity: 0.2; pointer-events: none; }
.job-card { 
  background: #222; padding: 12px 5px; border-radius: 8px; text-align: center; 
  cursor: pointer; border: 1px solid transparent; transition: 0.2s; 
}
.job-card:hover { background: #2a2a2a; }
.job-card.active { border-color: #409eff; background: rgba(64,158,255,0.1); box-shadow: 0 0 10px rgba(64,158,255,0.2); }
.job-card.locked { opacity: 0.3; cursor: not-allowed; pointer-events: none; }
.job-icon { font-size: 24px; margin-bottom: 4px; }
.sub { font-size: 12px; color: #ccc; }

/* 描述与属性信息 */
.info-section { margin-top: 25px; background: #222; padding: 15px; border-radius: 8px; border-top: 2px solid #444; }
.desc-box { font-size: 13px; line-height: 1.6; color: #bbb; margin-bottom: 12px; }
.attribute-info { font-size: 11px; color: #777; line-height: 1.6; }

/* 属性比例条 */
.stats-display { margin-top: 20px; background: #111; padding: 18px; border-radius: 10px; }
.stat-line { display: flex; align-items: center; gap: 12px; margin-bottom: 10px; }
.label { width: 35px; font-size: 12px; color: #999; }
.bar-outer { flex: 1; height: 8px; background: #222; border-radius: 4px; overflow: hidden; }
.bar-inner { height: 100%; transition: width 0.7s cubic-bezier(0.34, 1.56, 0.64, 1); }
.val-text { width: 25px; font-size: 12px; color: #eee; text-align: right; font-family: monospace; }

.footer { margin-top: 35px; }
.el-button--large { width: 100%; font-weight: bold; letter-spacing: 2px; height: 50px; }
</style>