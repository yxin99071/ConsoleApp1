<template>
  <div class="app-container">
    <h1>Arena 战斗模拟器</h1>
    
    <div v-if="loading">正在加载战斗数据...</div>
    
    <BattleReplay 
      v-else-if="battleData.length > 0" 
      :rawJson="battleData" 
    />
    
    <div v-else>
      <button @click="fetchBattleData">获取战斗数据</button>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted } from 'vue';
import BattleReplay from './components/BattleReplay.vue'; // 确保路径正确
import request from '@/api/request'; // 之前封装的 axios
import type { BattleEvent } from '@/Models/BattleInterface'; // 引入接口

const battleData = ref<BattleEvent[]>([]);
const loading = ref(false);

const fetchBattleData = async () => {
  loading.value = true;
  try {
    // 关键点：第一个传 any，第二个传你想要的最终类型 BattleEvent[]
    // 这样 TS 就会强制认为 data 是 BattleEvent[] 而不是 AxiosResponse
    const data = await request.get<any, BattleEvent[]>('/WeatherForecast/test');
    
    battleData.value = data;
  } catch (error) {
    console.error("无法获取战斗数据:", error);
  } finally {
    loading.value = false;
  }
};

// 如果你想页面一打开就加载
onMounted(() => {
  fetchBattleData();
});
</script>

<style>
.app-container {
  text-align: center;
  padding: 20px;
  background-color: #2c3e50;
  min-height: 100vh;
  color: white;
}
button {
  padding: 10px 20px;
  font-size: 16px;
  cursor: pointer;
}
</style>