# import numpy as np
# import matplotlib.pyplot as plt
# from matplotlib.widgets import Slider

# def logistic(x, max_val, mid, k):
#     # 基础计算
#     val = max_val / (1.0 + np.exp(-k * (x - mid)))
#     # 归零修正
#     offset = max_val / (1.0 + np.exp(k * mid))
#     return (val - offset) / (1.0 - offset / max_val)

# def double_logistic(x, m1, c1, k1, m2, c2, k2):
#     return logistic(x, m1, c1, k1) + logistic(x, m2, c2, k2)

# fig, ax = plt.subplots(figsize=(10, 6))
# plt.subplots_adjust(bottom=0.35)

# x = np.linspace(0, 200, 400)

# def update(val):
#     ax.clear()
    
#     # 获取滑块参数
#     m1 = s_m1.val # 第一段上限
#     c1 = s_c1.val # 第一段中点
#     k1 = s_k1.val # 第一段陡峭度
    
#     m2 = s_m2.val # 第二段上限
#     c2 = s_c2.val # 第二段中点
#     k2 = s_k2.val # 第二段陡峭度
    
#     y = double_logistic(x, m1, c1, k1, m2, c2, k2)
    
#     # 绘制总曲线
#     ax.plot(x, y, color='purple', lw=3, label='Total Dodge Rate')
    
#     # 绘制分量（虚线）用于理解
#     ax.plot(x, logistic(x, m1, c1, k1), '--', color='green', alpha=0.5, label='Phase 1 (Base)')
#     ax.plot(x, logistic(x, m2, c2, k2), '--', color='blue', alpha=0.5, label='Phase 2 (Mastery)')
    
#     ax.set_title("Staircase Dodge Curve (Double Logistic)")
#     ax.set_xlabel("Agility")
#     ax.set_ylabel("Dodge Rate")
#     ax.set_ylim(0, 0.5)
#     ax.grid(True)
#     ax.legend()
#     fig.canvas.draw_idle()

# # 滑块定义
# ax_m1 = plt.axes([0.1, 0.25, 0.3, 0.03]); s_m1 = Slider(ax_m1, 'P1 Max', 0.05, 0.3, valinit=0.15)
# ax_c1 = plt.axes([0.1, 0.20, 0.3, 0.03]); s_c1 = Slider(ax_c1, 'P1 Mid', 10, 100, valinit=20)
# ax_k1 = plt.axes([0.1, 0.15, 0.3, 0.03]); s_k1 = Slider(ax_k1, 'P1 Slope', 0.01, 0.2, valinit=0.1)

# ax_m2 = plt.axes([0.6, 0.25, 0.3, 0.03]); s_m2 = Slider(ax_m2, 'P2 Max', 0.05, 0.4, valinit=0.25)
# ax_c2 = plt.axes([0.6, 0.20, 0.3, 0.03]); s_c2 = Slider(ax_c2, 'P2 Mid', 50, 200, valinit=100)
# ax_k2 = plt.axes([0.6, 0.15, 0.3, 0.03]); s_k2 = Slider(ax_k2, 'P2 Slope', 0.01, 0.2, valinit=0.04)

# s_m1.on_changed(update); s_c1.on_changed(update); s_k1.on_changed(update)
# s_m2.on_changed(update); s_c2.on_changed(update); s_k2.on_changed(update)

# update(0)
# plt.show()
import numpy as np

# ==========================================
# 核心计算逻辑 (不要修改)
# ==========================================
def logistic(x, max_val, mid_point, k):
    if x <= 0: return 0
    raw = max_val / (1.0 + np.exp(-k * (x - mid_point)))
    offset = max_val / (1.0 + np.exp(k * mid_point))
    res = (raw - offset) / (1.0 - offset / max_val)
    return max(0, res)

def get_counter_rate(total_pts, job, p1, p2):
    # 属性分配 (8:1:1)
    if job == "Ranger":  agi, st, it = 0.8, 0.1, 0.1
    elif job == "Warrior": agi, st, it = 0.1, 0.8, 0.1
    else:                  agi, st, it = 0.1, 0.1, 0.8 # Mage
    
    # 1. 计算加权总值 (14:7:9)
    weighted = (14.0 * total_pts * agi + 7.0 * total_pts * st + 9.0 * total_pts * it) / 30.0
    
    # 2. 叠加双段曲线
    bonus = logistic(weighted, p1['max'], p1['mid'], p1['k']) + \
            logistic(weighted, p2['max'], p2['mid'], p2['k'])
            
    # 3. 基础 5% + 增量 (上限 20%)
    return min(0.20, 0.05 + bonus)

# ==========================================
# 在这里动态调整你的曲线参数
# ==========================================
# 第一段参数：控制前期爆发
phase1 = {
    'max': 0.08,    # 第一段增量上限 (0.08 = 8%)
    'mid': 35,      # 第一段中点 (加权属性到 35 时收益过半)
    'k':   0.08     # 陡峭度 (越大爆发越快)
}

# 第二段参数：控制后期突破
phase2 = {
    'max': 0.07,    # 第二段增量上限
    'mid': 90,      # 第二段中点
    'k':   0.05     # 陡峭度
}

# ==========================================
# 输出结果展示
# ==========================================
def display_results():
    nodes = [50, 100, 200]
    jobs = ["Ranger", "Warrior", "Mage"]
    
    header = f"{'职业':<10} | {'属性: 50':<12} | {'属性: 100':<12} | {'属性: 200':<12}"
    print("\n" + "="*55)
    print(f"当前参数: P1(Mid:{phase1['mid']} K:{phase1['k']}) | P2(Mid:{phase2['mid']} K:{phase2['k']})")
    print("-" * 55)
    print(header)
    print("-" * 55)
    
    for job in jobs:
        row = []
        for pts in nodes:
            rate = get_counter_rate(pts, job, phase1, phase2)
            row.append(f"{rate*100:>10.2f}%")
        print(f"{job:<10} | {' | '.join(row)}")
    print("="*55)

# 执行展示
display_results()