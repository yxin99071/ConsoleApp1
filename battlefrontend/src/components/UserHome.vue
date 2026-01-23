<template>
    <div class="common-layout">
        <el-container class="home-container">
            <el-aside width="200px" class="aside">
                <div class="menu-title">英雄大厅</div>
                <el-menu :default-active="activeMenu" @select="handleMenuSelect">
                    <el-menu-item index="info">
                        <el-icon>
                            <User />
                        </el-icon>
                        <span>我的信息</span>
                    </el-menu-item>
                    <el-menu-item index="weapon">
                        <el-icon>
                            <Compass />
                        </el-icon>
                        <span>武器图鉴</span>
                    </el-menu-item>
                    <el-menu-item index="skill">
                        <el-icon>
                            <Lightning />
                        </el-icon>
                        <span>技能图鉴</span>
                    </el-menu-item>
                    <el-menu-item index="battle">
                        <el-icon>
                            <Sword />
                        </el-icon>
                        <span>前往战斗</span>
                    </el-menu-item>
                </el-menu>
            </el-aside>

            <el-main class="main-content">
                <div v-if="activeMenu === 'info'" class="profile-section">
                    <el-card class="profile-card">
                        <template #header>
                            <div class="card-header">
                                <span class="char-name">{{ userData.name }}</span>
                                <el-tag effect="dark">{{ userData.profession }}</el-tag>
                            </div>
                        </template>

                        <el-descriptions :column="2" border>
                            <el-descriptions-item label="等级">
                                <b style="color: #409eff">LV. {{ userData.level }}</b>
                            </el-descriptions-item>

                            <el-descriptions-item label="职业">
                                <el-tag size="small">{{ userData.profession }}</el-tag>
                            </el-descriptions-item>

                            <el-descriptions-item label="经验进度" :span="2">
                                <div class="exp-container">
                                    <div class="exp-text">
                                        <span>{{ userData.exp }}</span>
                                        <span class="exp-separator">/</span>
                                        <span class="exp-target">{{ expThreshold }}</span>
                                    </div>
                                    <el-progress :percentage="expPercentage" :stroke-width="12" striped striped-flow
                                        color="#67c23a" />
                                </div>
                            </el-descriptions-item>

                            <el-descriptions-item label="第二职业">
                                {{ userData.secondProfession || '尚未解锁' }}
                            </el-descriptions-item>

                            <el-descriptions-item label="账号ID">
                                {{ editForm.account }}
                            </el-descriptions-item>
                        </el-descriptions>

                        <div class="preview-sections">

                            <div class="category-block">
                                <h4 class="section-title"><el-icon>
                                        <Compass />
                                    </el-icon> 武器图鉴</h4>
                                <div class="tag-group">
                                    <el-popover v-for="wp in userData.weaponDTO" :key="wp.name" placement="top-start"
                                        :width="280" trigger="click" :persistent="false">
                                        <template #reference>
                                            <el-tag class="item-tag" type="warning" effect="dark"
                                                style="cursor: pointer">
                                                {{ wp.name }}
                                            </el-tag>
                                        </template>

                                        <div class="popover-main">
                                            <div class="popover-desc">
                                                <div class="item-title">{{ wp.name }}</div>
                                                {{ wp.description }}
                                            </div>
                                            <div v-if="wp.buffs.length" class="buff-container">
                                                <div class="buff-label">点击图标查看属性:</div>
                                                <div class="buff-tags">
                                                    <el-popover v-for="buff in wp.buffs" :key="buff.name"
                                                        placement="right" :width="200" trigger="click">
                                                        <template #reference>
                                                            <el-tag size="small" class="inner-buff-tag" effect="plain">
                                                                {{ buff.name }}
                                                            </el-tag>
                                                        </template>
                                                        <div class="buff-detail">
                                                            <strong style="color: #409eff">{{ buff.name }}</strong>
                                                            <p>{{ buff.description }}</p>
                                                        </div>
                                                    </el-popover>
                                                </div>
                                            </div>
                                        </div>
                                    </el-popover>
                                </div>
                            </div>

                            <el-divider />

                            <div class="category-block">
                                <h4 class="section-title"><el-icon>
                                        <Lightning />
                                    </el-icon> 技能图鉴</h4>
                                <div class="tag-group">
                                    <el-popover v-for="skill in userData.skillDTO" :key="skill.name"
                                        placement="top-start" :width="280" trigger="click" :persistent="false">
                                        <template #reference>
                                            <el-tag class="item-tag" type="warning" effect="dark"
                                                style="cursor: pointer">
                                                {{ skill.name }}
                                            </el-tag>
                                        </template>

                                        <div class="popover-main">
                                            <div class="popover-desc">
                                                <div class="item-title">{{ skill.name }}</div>
                                                {{ skill.description }}
                                            </div>
                                            <div v-if="skill.buffs && skill.buffs.length" class="buff-container">
                                                <div class="buff-label">点击图标查看属性:</div>
                                                <div class="buff-tags">
                                                    <el-popover v-for="buff in skill.buffs" :key="buff.name"
                                                        placement="right" :width="200" trigger="click">
                                                        <template #reference>
                                                            <el-tag size="small" class="inner-buff-tag" effect="plain">
                                                                {{ buff.name }}
                                                            </el-tag>
                                                        </template>
                                                        <div class="buff-detail">
                                                            <strong style="color: #409eff">{{ buff.name }}</strong>
                                                            <p>{{ buff.description }}</p>
                                                        </div>
                                                    </el-popover>
                                                </div>
                                            </div>
                                        </div>
                                    </el-popover>
                                </div>
                            </div>
                        </div>

                        <el-button type="primary" icon="Edit" circle class="edit-btn" @click="openEditDialog" />
                    </el-card>
                </div>

                <div v-else class="placeholder">
                    <el-empty :description="menuNameMap[activeMenu] + ' 正在建设中...'" />
                </div>
            </el-main>
        </el-container>

        <el-dialog v-model="editDialogVisible" title="修改基本信息" width="400px">
            <el-form :model="editForm" label-width="80px">
                <el-form-item label="账号">
                    <el-input v-model="editForm.account" />
                </el-form-item>
                <el-form-item label="昵称">
                    <el-input v-model="editForm.name" />
                </el-form-item>
            </el-form>
            <template #footer>
                <el-button @click="editDialogVisible = false">取消</el-button>
                <el-button type="primary" @click="handleUpdateProfile" :loading="updateLoading">确认修改</el-button>
            </template>
        </el-dialog>
    </div>
</template>

<script setup lang="ts">
import { ref, reactive, onMounted, computed } from 'vue';
import { ElMessage } from 'element-plus';
import { getProfileApi, updateProfileApi } from '@/api/user';
import type { Buff, GameItem, UserProfile } from '@/Models/BattleInterface'

const activeMenu = ref('info');
const editDialogVisible = ref(false);
const updateLoading = ref(false);
const currentAccount = ref(localStorage.getItem('userAccount') || ''); // 假设存在本地

const menuNameMap: any = {
    info: '我的信息',
    weapon: '武器图鉴',
    skill: '技能图鉴',
    battle: '前往战斗'
};

// 角色数据初始结构
const userData = reactive<UserProfile>({
    name: '',
    exp: 0,
    level: 0,
    profession: '',
    secondProfession: null,
    skillDTO: [], // 此时 TS 知道这里以后会存 GameItem 类型的数据，不再是 never
    weaponDTO: []
});

// 修改表单
const editForm = reactive({
    account: '',
    name: ''
});

// 初始化获取数据
const fetchProfile = async () => {
    try {
        // 强制断言 res 的类型为 UserProfile
        const res = await getProfileApi() as unknown as UserProfile;

        // 现在赋值就不会报错了
        Object.assign(userData, res);

        editForm.name = res.name;
        editForm.account = currentAccount.value;
    } catch (error) {
        console.error('获取资料失败');
    }
};

const handleMenuSelect = (index: string) => {
    activeMenu.value = index;
};

const openEditDialog = () => {
    editDialogVisible.value = true;
};

const handleUpdateProfile = async () => {
    updateLoading.value = true;
    try {
        await updateProfileApi(editForm);
        ElMessage.success('更新成功');
        userData.name = editForm.name; // 同步更新本地显示
        currentAccount.value = editForm.account;
        localStorage.setItem('userAccount', editForm.account);
        editDialogVisible.value = false;
    } finally {
        updateLoading.value = false;
    }
};
const expThreshold = computed(() => {
    const nextLevel = userData.level + 1;
    return 50.0 * Math.pow(nextLevel, 2) + 50.0 * nextLevel;
});

// 计算百分比，用于进度条展示 (可选)
const expPercentage = computed(() => {
    if (expThreshold.value === 0) return 0;
    const percent = (userData.exp / expThreshold.value) * 100;
    return Math.min(100, Math.round(percent)); // 最高100%
});
onMounted(() => {
    fetchProfile();
});
</script>

<style scoped>
.exp-container {
    width: 100%;
    padding: 5px 0;
}

.exp-text {
    font-family: 'Courier New', Courier, monospace;
    /* 使用等宽字体更有数值感 */
    font-weight: bold;
    font-size: 16px;
    margin-bottom: 8px;
    display: flex;
    align-items: center;
    gap: 5px;
}

.exp-separator {
    color: #909399;
    font-size: 14px;
}

.exp-target {
    color: #f56c6c;
    /* 目标数值用红色或深色标注 */
}

/* 进度条动画效果 */
:deep(.el-progress-bar__inner) {
    transition: width 0.6s ease;
}

.home-container {
    height: 100vh;
    background-color: #f5f7fa;
}

.aside {
    background-color: #fff;
    border-right: 1px solid #dcdfe6;
}

.menu-title {
    padding: 20px;
    font-size: 18px;
    font-weight: bold;
    color: #409eff;
    text-align: center;
}

.main-content {
    padding: 30px;
}

.profile-card {
    max-width: 800px;
    margin: 0 auto;
    position: relative;
}

.card-header {
    display: flex;
    justify-content: space-between;
    align-items: center;
}

.char-name {
    font-size: 24px;
    font-weight: bold;
}

.preview-sections {
    margin-top: 25px;
}

.tag-group {
    display: flex;
    flex-wrap: wrap;
    gap: 12px;
}

.item-tag {
    cursor: pointer;
    padding: 15px;
    font-size: 14px;
}

.buff-list {
    display: flex;
    flex-wrap: wrap;
    gap: 5px;
    margin-top: 8px;
}

.buff-tag {
    cursor: help;
}

.edit-btn {
    position: absolute;
    bottom: 20px;
    right: 20px;
}

.buff-desc {
    font-size: 12px;
    color: #666;
}

.placeholder {
    height: 100%;
    display: flex;
    justify-content: center;
    align-items: center;
}

/* 标题样式 */
.section-title {
    display: flex;
    align-items: center;
    gap: 8px;
    color: #606266;
    margin-bottom: 15px;
}

/* 标签基础样式 */
.item-tag {
    cursor: pointer;
    transition: all 0.2s;
    padding: 18px 12px;
    /* 增大点击/悬浮区域 */
    font-weight: bold;
}

.item-tag:hover {
    transform: translateY(-2px);
    /* 悬浮时轻微上浮，给用户反馈 */
    box-shadow: 0 2px 8px rgba(0, 0, 0, 0.1);
}

/* Popover 内容样式 */
.popover-main {
    padding: 5px;
}

.popover-desc {
    font-size: 14px;
    line-height: 1.6;
    color: #333;
    margin-bottom: 12px;
    border-bottom: 1px solid #eee;
    padding-bottom: 8px;
}

.buff-label {
    font-size: 12px;
    color: #909399;
    margin-bottom: 6px;
}

.buff-tags {
    display: flex;
    flex-wrap: wrap;
    gap: 6px;
}

.inner-buff-tag {
    cursor: help;
    border: 1px solid #dcdfe6;
    color: #555;
}

.buff-detail {
    font-size: 12px;
    color: #409eff;
    font-style: italic;
}
</style>