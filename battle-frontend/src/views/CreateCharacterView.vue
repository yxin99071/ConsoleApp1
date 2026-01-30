<script setup lang="ts">
import { ref, computed, watch } from 'vue';
import { useRouter } from 'vue-router';
import ItemCard from '../components/game/ItemCard.vue';
import type { InitProfileDto } from '../types/battle';
import { getProfile, initProfile } from '../api/battle';
import gsap from 'gsap';

const router = useRouter();
const canConfirm = computed(() => {
    if (!name.value.trim()) return false; // 必须都有值
    if (mainJob.value === '凡人') return true;
    return mainJob.value !== '' && subJob.value !== '' && selectedSkill.value !== '';
});

function keyToString(key: any): string {
    var stringKey = key.toString()
    if (stringKey === 'agi') {
        return '敏捷'
    }
    if (stringKey == 'str') {
        return '力量'
    }
    if (stringKey == 'int') {
        return '智力'
    }
    return 'UNKONWN'
}


const name = ref('');
const mainJob = ref<'战士' | '游侠' | '法师' | '凡人' | ''>('');
const subJob = ref<'战士' | '游侠' | '法师' | ''>('');
const selectedSkill = ref<'假死' | '亡者意志' | ''>('');

const JOB_DESC = {
    '战士': '拥有极高的生命值与暴击伤害，为暴击率添加少部分力量修正。',
    '游侠': '倾向于更多的行动，反击闪避率较高，为暴击伤害提供少部分敏捷修正。',
    '法师': '不依赖暴击，根据智力百分比修正伤害，开局会获得持续1回合的护盾',
    '凡人': '平衡发展，升级获得额外属性点，可以使用所有职业技能与武器，但由于单属性加成低，效果不佳。',
    '': '请选择你的初始职业。'
};

const STAT_HINTS = {
    'STR': '【力量】提升基础生命上限与暴击伤害。',
    'AGI': '【敏捷】影响出手速度、闪避率以及暴击率。',
    'INT': '【智力】按照一定比例修正伤害。',
};

watch(mainJob, (newJob) => {
    if (newJob === '凡人') { subJob.value = ''; selectedSkill.value = ''; }
});

const stats = computed(() => {
    if (!mainJob.value) return { str: 20, agi: 20, int: 20 };
    if (mainJob.value === '凡人') return { str: 24, agi: 24, int: 24 };
    let s = 9, a = 9, i = 9;
    const isPure = !subJob.value || mainJob.value === subJob.value;
    if (isPure) {
        if (mainJob.value === '战士') s = 42;
        else if (mainJob.value === '游侠') a = 42;
        else if (mainJob.value === '法师') i = 42;
    } else {
        if (mainJob.value === '战士') s = 36; else if (mainJob.value === '游侠') a = 36; else if (mainJob.value === '法师') i = 36;
        if (subJob.value === '战士') s = 15; else if (subJob.value === '游侠') a = 15; else if (subJob.value === '法师') i = 15;
    }
    return { str: s, agi: a, int: i };
});

const SKILL_DATABASE: Record<string, any> = {
    '假死': {
        name: '假死', profession: '游侠', isPassive: true,
        description: '生命值低于0时，净化所有效果，回复至1点生命',
        buffs: []
    },
    '亡者意志': {
        name: '亡者意志', profession: '战士', isPassive: true,
        description: '生命值低于0时，获得伤害增幅，立刻反击获得50%生命偷取，跳过自己下个回合。',
        buffs: []
    }
};

const showConfirmModal = ref(false);

// 1. 点击按钮时，不直接请求，而是打开弹窗
const handleCreate = () => {
    if (!canConfirm.value) return;
    showConfirmModal.value = true;
};

// 2. 弹窗内的“确认”按钮触发的真正逻辑
const executeCreate = async () => {
    // 先关闭弹窗，确保动画流畅
    showConfirmModal.value = false;
    
    // ... 你原本的请求逻辑
    const dto: InitProfileDto = {
        name: name.value,
        account: '',
        profession: mainJob.value,
        secondProfession: mainJob.value === '凡人' ? null : (subJob.value || null),
        initialSkills: mainJob.value === '凡人' ? ['假死', '亡者意志'] : [selectedSkill.value]
    };

    try {
        const res = await initProfile(dto);
        if (res) {
            getProfile();
            router.push('/lobby');
        }
    } catch (error) {
        console.error('Initialization failed', error);
    }
};

// 3. GSAP 动画钩子
const onEnter = (el: Element, done: () => void) => {
    const box = el.querySelector('#confirm-box');
    const bg = el.querySelector('.modal-bg');
    
    // 初始化状态
    gsap.set(bg, { opacity: 0 });
    gsap.set(box, { y: 40, opacity: 0, scale: 0.95 });

    const tl = gsap.timeline({ onComplete: done });
    tl.to(bg, { opacity: 1, duration: 0.3 })
      .to(box, { y: 0, opacity: 1, scale: 1, duration: 0.4, ease: "power2.out" }, "-=0.2");
};

const onLeave = (el: Element, done: () => void) => {
    const box = el.querySelector('#confirm-box');
    const bg = el.querySelector('.modal-bg');
    
    const tl = gsap.timeline({ onComplete: done }); // 这里的 done() 必须在动画完成后执行
    tl.to(box, { y: 20, opacity: 0, scale: 0.95, duration: 0.3, ease: "power2.in" })
      .to(bg, { opacity: 0, duration: 0.2 }, "-=0.1");
};

</script>
<template>
    <div
        class="h-screen w-full bg-[#020617] flex items-center justify-center p-8 overflow-hidden text-slate-300 relative font-sans">
        <div class="absolute inset-0 bg-grid-pattern opacity-10"></div>

        <div class="relative w-full max-w-7xl h-[85vh] grid grid-cols-[1.2fr_1fr] gap-12 items-stretch">

            <section class="flex flex-col gap-8 overflow-y-auto pr-6 custom-scrollbar">

                <div class="space-y-6">
                    <div class="space-y-1">
                        <p class="text-[13px] font-bold text-indigo-500 tracking-[0.3em] uppercase opacity-60 px-1">
                            角色名</p>
                        <input v-model="name" placeholder="输入角色名"
                            class="bg-transparent text-5xl font-black text-slate-300 outline-none border-b border-white/10 focus:border-indigo-500 w-full placeholder:text-slate-300 transition-all py-2" />
                    </div>
                </div>

                <div class="bg-slate-900/40 border border-white/5 p-8 rounded-3xl space-y-6">
                    <div v-for="(val, key) in stats" :key="key" class="space-y-2">
                        <div class="flex justify-between items-end px-1">
                            <span class="text-[14px] font-black uppercase tracking-widest text-slate-500">{{
                                keyToString(key)
                                }}</span>
                            <span class="font-mono text-2xl font-bold"
                                :class="key === 'str' ? 'text-red-500' : key === 'agi' ? 'text-green-500' : 'text-purple-500'">{{
                                    val }}</span>
                        </div>
                        <div class="h-2.5 bg-black/60 rounded-full overflow-hidden border border-white/5 p-0.5">
                            <div class="h-full rounded-full transition-all duration-700 shadow-lg"
                                :class="key === 'str' ? 'bg-red-500 shadow-red-900/40' : key === 'agi' ? 'bg-green-500 shadow-green-900/40' : 'bg-purple-500 shadow-purple-900/40'"
                                :style="{ width: (val / 60 * 100) + '%' }"></div>
                        </div>
                        <p class="text-[14px] text-slate-400 italic px-1 opacity-60">{{ STAT_HINTS[key.toUpperCase() as
                            keyof typeof STAT_HINTS] }}</p>
                    </div>
                </div>

                <div class="flex flex-col gap-8">
                    <div class="w-full space-y-3">
                        <div class="p-6 bg-indigo-500/5 border border-indigo-500/20 rounded-2xl">
                            <h4 class="text-[13px] font-black text-indigo-400 mb-2 uppercase tracking-tighter italic">
                                职业 说明</h4>
                            <p class="text-sm leading-relaxed text-slate-300 italic">{{ JOB_DESC[mainJob || ''] }}</p>
                        </div>
                        <p class="text-[13px] text-slate-400 flex items-center gap-2 px-2">
                            <span class="w-1.5 h-1.5 rounded-full bg-indigo-500 animate-pulse"></span>
                            {{ mainJob === '凡人' ? '凡人进阶：成长点数 +1 (每级7点)' : '基础职业：固定成长 (每级6点)' }}
                        </p>
                    </div>

                    <div class="w-full space-y-4">
                        <h4 class="text-[13px] font-black text-slate-500 uppercase tracking-widest px-1">初始技能</h4>
                        <div class="flex gap-8 items-start min-h-60">
                            <template v-if="mainJob === '凡人'">
                                <ItemCard :item="SKILL_DATABASE['假死']" type="技能" class="scale-[0.85] origin-top-left" />
                                <ItemCard :item="SKILL_DATABASE['亡者意志']" type="技能"
                                    class="scale-[0.85] origin-top-left" />
                            </template>

                            <template v-else-if="selectedSkill">
                                <ItemCard :item="SKILL_DATABASE[selectedSkill]" type="技能"
                                    class="scale-[0.9] origin-top-left" />
                            </template>

                            <div v-else
                                class="w-48 h-64 border-2 border-dashed border-white/5 rounded-2xl flex items-center justify-center text-[13px] text-slate-800 text-center uppercase tracking-tighter">
                                请选择
                            </div>
                        </div>
                    </div>
                </div>
            </section>

            <section class="flex flex-col relative pl-12 border-l border-white/5">
                <div class="space-y-10 flex-1 overflow-y-auto pr-4 custom-scrollbar pb-32">
                    <div class="space-y-4">
                        <h3 class="text-[14px] font-black tracking-[0.4em] text-slate-500 uppercase">01. 主职业
                        </h3>
                        <div class="grid grid-cols-2 gap-3">
                            <button v-for="j in (['战士', '游侠', '法师', '凡人'] as const)" :key="j" @click="mainJob = j"
                                :class="mainJob === j ? 'bg-indigo-600 border-indigo-400 text-white shadow-xl shadow-indigo-900/20' : 'bg-white/5 border-white/10 text-slate-500'"
                                class="py-4 rounded-xl border font-black transition-all active:scale-95">{{ j
                                }}</button>
                        </div>
                    </div>

                    <div class="space-y-4 transition-opacity"
                        :class="{ 'opacity-20 pointer-events-none': mainJob === '凡人' || !mainJob }">
                        <h3 class="text-[14px] font-black tracking-[0.4em] text-slate-500 uppercase">02. 副职业
                        </h3>
                        <div class="grid grid-cols-3 gap-3">
                            <button v-for="j in (['战士', '游侠', '法师'] as const)" :key="j" @click="subJob = j"
                                :class="subJob === j ? 'bg-indigo-600 border-indigo-400 text-white' : 'bg-white/5 border-white/10 text-slate-500'"
                                class="py-3 rounded-xl border font-black transition-all active:scale-95">{{ j
                                }}</button>
                        </div>
                    </div>

                    <div class="space-y-4 transition-opacity"
                        :class="{ 'opacity-20 pointer-events-none': mainJob === '凡人' || !mainJob }">
                        <h3 class="text-[14px] font-black tracking-[0.4em] text-slate-500 uppercase">03. 初始技能
                        </h3>
                        <div class="flex gap-3">
                            <button v-for="s in (['假死', '亡者意志'] as const)" :key="s" @click="selectedSkill = s"
                                :class="selectedSkill === s ? 'bg-amber-600 border-amber-400 text-white shadow-xl shadow-amber-900/20' : 'bg-white/5 border-white/10 text-slate-500'"
                                class="flex-1 py-4 rounded-xl border font-black transition-all italic active:scale-95">{{
                                    s
                                }}</button>
                        </div>
                        <p class="text-[14px] text-slate-500 italic bg-white/5 p-4 rounded-xl leading-relaxed">{{
                            selectedSkill ?
                                SKILL_DATABASE[selectedSkill].description : '选定主职后可解锁初始技能选择。' }}</p>
                    </div>
                </div>

                <div class="absolute bottom-0 right-0 p-4">
                    <button @click="handleCreate" :disabled="!canConfirm"
                        class="relative w-32 h-32 rounded-full border-4 flex flex-col items-center justify-center font-black italic transition-all duration-700 group overflow-hidden"
                        :class="canConfirm ? 'bg-white text-black border-white shadow-[0_0_50px_rgba(255,255,255,0.3)] cursor-pointer' : 'bg-slate-900 text-white/5 border-white/5 cursor-not-allowed'">
                        <span class="text-[9px] tracking-widest group-hover:animate-pulse">确定</span>
                        <span class="text-2xl">START</span>
                        <div v-if="canConfirm"
                            class="absolute inset-0 bg-linear-to-tr from-transparent via-white/30 to-transparent -translate-x-full group-hover:translate-x-full transition-transform duration-1000">
                        </div>
                    </button>
                    <Transition @enter="onEnter" @leave="onLeave" :css="false">
                        <div v-if="showConfirmModal" class="fixed inset-0 z-50 flex items-center justify-center p-4">
                            <div @click="showConfirmModal = false"
                                class="modal-bg absolute inset-0 bg-black/80 backdrop-blur-sm">
                            </div>

                            <div id="confirm-box"
                                class="relative bg-zinc-900 border border-white/20 p-8 w-full max-w-sm text-center shadow-2xl">
                                <h3 class="text-white text-xl font-black italic tracking-tighter mb-4 text-shadow-glow">
                                    WARNING</h3>

                                <p class="text-zinc-400 text-sm leading-relaxed mb-8">
                                    职业一旦觉醒，<span class="text-white font-bold underline">命运轨迹将无法逆转</span>。<br>
                                    你确定要以此身份开启征程吗？
                                </p>

                                <div class="flex flex-col gap-3">
                                    <button @click="executeCreate"
                                        class="w-full bg-white text-black py-3 font-black text-sm hover:invert transition-all duration-300 uppercase">
                                        确认觉醒
                                    </button>
                                    <button @click="showConfirmModal = false"
                                        class="w-full bg-transparent text-zinc-500 py-2 text-xs hover:text-white transition-colors uppercase tracking-widest">
                                        [ 返回重新选择 ]
                                    </button>
                                </div>
                            </div>
                        </div>
                    </Transition>
                </div>
            </section>
        </div>
    </div>

</template>
<style scoped>
.bg-grid-pattern {
    background-image:
        linear-gradient(to right, rgba(255, 255, 255, 0.05) 1px, transparent 1px),
        linear-gradient(to bottom, rgba(255, 255, 255, 0.05) 1px, transparent 1px);
    background-size: 50px 50px;
    mask-image: radial-gradient(circle at center, black, transparent 90%);
}

.custom-scrollbar::-webkit-scrollbar {
    width: 4px;
}

.custom-scrollbar::-webkit-scrollbar-track {
    background: transparent;
}

.custom-scrollbar::-webkit-scrollbar-thumb {
    background: rgba(255, 255, 255, 0.05);
    border-radius: 13px;
}

.custom-scrollbar::-webkit-scrollbar-thumb:hover {
    background: rgba(255, 255, 255, 0.1);
}

/* Start Button Glow Effect */
@keyframes pulse-white {
    0% {
        transform: scale(1);
        box-shadow: 0 0 0 0 rgba(255, 255, 255, 0.4);
    }

    70% {
        transform: scale(1.05);
        box-shadow: 0 0 0 20px rgba(255, 255, 255, 0);
    }

    100% {
        transform: scale(1);
        box-shadow: 0 0 0 0 rgba(255, 255, 255, 0);
    }
}

button:not(:disabled):hover {
    filter: brightness(1.1);
}

.shadow-\[0_0_50px_rgba\(255\,255\,255\,0\.3\)\] {
    animation: pulse-white 2s infinite;
}
</style>