<script setup lang="ts">
import { ref } from 'vue';
import { useRouter } from 'vue-router';
import { login } from '../api/auth';
import type { LoginDTO } from '../types/auth';

const router = useRouter();
const form = ref<LoginDTO>({
  Account: '',
  Password: ''
});

const loading = ref(false);
const errorMsg = ref('');

const handleLogin = async () => {
  if (loading.value) return;
  loading.value = true;
  errorMsg.value = '';
  try {
    const data = await login(form.value);
    localStorage.setItem('token', data.token);
    localStorage.setItem('userId', data.id);
    localStorage.setItem('userName', data.name);
    localStorage.setItem('userProfession', data.profession ?? 'ERROR');

    if (!data.profession) {
      router.push('/create-character');
    } else {
      router.push('/lobby');
    }
  } catch (err: any) {
    errorMsg.value = err.response?.data || '账号或密码错误';
  } finally {
    loading.value = false;
  }
};
</script>

<template>
  <div class="min-h-screen bg-[#020617] flex items-center justify-center p-4 overflow-hidden relative">
    <div class="absolute inset-0 z-0">
      <div class="absolute inset-0 bg-[radial-gradient(circle_at_50%_50%,#1e1b4b_0%,#020617_100%)] opacity-60"></div>
      <div class="absolute inset-0" style="background-image: radial-gradient(#ffffff05 1px, transparent 1px); background-size: 40px 40px;"></div>
    </div>

    <div class="w-full max-w-4xl bg-slate-900/40 border border-white/10 rounded-2xl shadow-[0_0_50px_rgba(0,0,0,0.5)] backdrop-blur-2xl z-10 flex overflow-hidden flex-col md:flex-row">
      
      <div class="w-full md:w-1/2 p-12 flex flex-col justify-center border-r border-white/5 bg-indigo-600/5">
        <div class="space-y-4">
          <div class="w-12 h-1 bg-indigo-500"></div>
          <h1 class="text-5xl font-black text-white tracking-tighter leading-none">BATTLE<br/><span class="text-indigo-500">SYSTEM</span></h1>
          <p class="text-slate-400 text-sm font-mono tracking-widest uppercase">Version 2.0.26 // Secure Access</p>
        </div>
        <div class="mt-20 text-[10px] text-slate-600 font-mono italic">
          &gt; INITIALIZING ENCRYPTION...<br/>
          &gt; WAITING FOR CREDENTIALS...
        </div>
      </div>

      <div class="w-full md:w-1/2 p-12 bg-black/20">
        <h2 class="text-xl font-bold text-white mb-8 tracking-tight">Identity Verification</h2>
        
        <form @submit.prevent="handleLogin" class="space-y-6">
          <div class="group">
            <label class="block text-[10px] text-slate-500 font-bold uppercase mb-2 tracking-[0.2em] group-focus-within:text-indigo-400 transition-colors">Account</label>
            <input 
              v-model="form.Account" 
              type="text" 
              required
              placeholder="Identity UID"
              class="w-full bg-slate-950/50 border border-slate-800 rounded-lg px-4 py-3 text-white focus:border-indigo-500 focus:ring-1 focus:ring-indigo-500/50 outline-none transition-all placeholder:text-slate-800 font-mono"
            >
          </div>

          <div class="group">
            <label class="block text-[10px] text-slate-500 font-bold uppercase mb-2 tracking-[0.2em] group-focus-within:text-indigo-400 transition-colors">Password</label>
            <input 
              v-model="form.Password" 
              type="password" 
              required
              placeholder="••••••••"
              class="w-full bg-slate-950/50 border border-slate-800 rounded-lg px-4 py-3 text-white focus:border-indigo-500 focus:ring-1 focus:ring-indigo-500/50 outline-none transition-all placeholder:text-slate-800"
            >
          </div>

          <Transition name="slide-down">
            <div v-if="errorMsg" class="bg-red-500/10 border-l-2 border-red-500 text-red-500 text-[11px] py-2 px-3 flex items-center gap-2 italic">
              <span>[!]</span> {{ errorMsg }}
            </div>
          </Transition>

          <button 
            :disabled="loading" 
            type="submit"
            class="w-full group relative overflow-hidden bg-indigo-600 hover:bg-indigo-500 disabled:bg-slate-800 text-white font-bold py-4 rounded-lg transition-all active:scale-[0.98]"
          >
            <div class="absolute inset-0 bg-linear-to-r from-transparent via-white/10 to-transparent -translate-x-full group-hover:animate-[shimmer_1.5s_infinite]"></div>
            <span v-if="!loading" class="tracking-widest uppercase text-xs">Enter Battlefield</span>
            <span v-else class="flex items-center justify-center gap-2 text-xs">
              <svg class="animate-spin h-4 w-4 text-white" fill="none" viewBox="0 0 24 24"><circle class="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" stroke-width="4"></circle><path class="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z"></path></svg>
              AUTHENTICATING...
            </span>
          </button>
        </form>
      </div>
    </div>
  </div>
</template>

<style scoped>
@keyframes shimmer {
  100% { transform: translateX(100%); }
}

.slide-down-enter-active, .slide-down-leave-active { transition: all 0.3s ease; }
.slide-down-enter-from, .slide-down-leave-to { opacity: 0; transform: translateY(-10px); }
</style>