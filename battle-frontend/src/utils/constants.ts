// src/utils/constants.ts
export const PROFESSION_MAP: Record<string, { icon: string, color: string, label: string }> = {
  MORTAL:   { icon: '👤', color: 'text-slate-400', label: '凡人' },
  WARRIOR:  { icon: '⚔️', color: 'text-red-500',   label: '战士' },
  RANGER:   { icon: '🏹', color: 'text-green-500', label: '游侠' },
  MAGICIAN: { icon: '🔮', color: 'text-purple-500', label: '法师' }
};