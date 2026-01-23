<template>
  <el-config-provider>
    <div id="app">
      <router-view />
    </div>
  </el-config-provider>
</template>

<script setup lang="ts">
// 这里目前不需要复杂的逻辑
</script>

<style>
/* 全局样式重置 */
html, body {
  margin: 0;
  padding: 0;
  height: 100%;
  font-family: 'Helvetica Neue', Helvetica, 'PingFang SC', 'Hiragino Sans GB', 'Microsoft YaHei', Arial, sans-serif;
}

#app {
  height: 100vh;
  width: 100vw;
}

/* 消除 Element Plus 的一些默认边距干扰 */
.el-main {
  padding: 0;
}
</style>