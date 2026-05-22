// src/utils/constants.ts

export const PROFESSION_MAP: Record<string, {
  icon:         string  // emoji icon
  label:        string  // Chinese name
  color:        string  // legacy text color (keep for backward compat)
  headerBg:     string  // card header background tint
  badgeBg:      string  // profession badge background
  badgeBorder:  string  // profession badge border
  badgeText:    string  // profession badge text color
  stampOpacity: string  // placeholder stamp opacity (swap for img when art is ready)
}> = {
  MORTAL: {
    icon:         '👤',
    label:        '凡人',
    color:        'text-slate-400',
    headerBg:     'bg-slate-700/50',
    badgeBg:      'bg-slate-700/90',
    badgeBorder:  'border-slate-400/50',
    badgeText:    'text-slate-200',
    stampOpacity: 'opacity-[0.05]',
  },
  WARRIOR: {
    icon:         '⚔️',
    label:        '战士',
    color:        'text-red-400',
    headerBg:     'bg-red-950/60',
    badgeBg:      'bg-red-900/90',
    badgeBorder:  'border-red-400/50',
    badgeText:    'text-red-200',
    stampOpacity: 'opacity-[0.06]',
  },
  RANGER: {
    icon:         '🏹',
    label:        '游侠',
    color:        'text-emerald-400',
    headerBg:     'bg-emerald-950/60',
    badgeBg:      'bg-emerald-900/90',
    badgeBorder:  'border-emerald-400/50',
    badgeText:    'text-emerald-200',
    stampOpacity: 'opacity-[0.06]',
  },
  MAGICIAN: {
    icon:         '🔮',
    label:        '法师',
    color:        'text-violet-400',
    headerBg:     'bg-violet-950/70',
    badgeBg:      'bg-violet-900/90',
    badgeBorder:  'border-violet-400/50',
    badgeText:    'text-violet-200',
    stampOpacity: 'opacity-[0.07]',
  },
}
