# SmartcarPlatform  [![Build Status](https://travis-ci.com/potterhere/SmartCarPlatform.svg?branch=master)](https://travis-ci.com/potterhere/SmartCarPlatform)
智能车调试平台，主要为了解决普通智能车参数调试效率低的问题，在此基础上提出的一个解决方案。

![上位机](./Freescale_debug/Source/master.png)

**主要功能：**

- 实时绘制运行数据、显示变量数值
- 修改PID参数
- 修改自定义参数
- 绘制CCD图像（最大支持三个CCD，单个CCD数据大小1*128）
- **绘制摄像头图像（目前仅支持分辨率80*60）**

**Todo:**

- [ ] 代码重构，优化软件结构
- [ ] 数据分析（模仿Mission Planner的Log分析）
- [ ] 可调用自定义算法
- [x] 优化调试方法（如Log4Net控件等）
 

**对应的下位机程序见Github：[K60以及KL26](https://github.com/potterhere/Freescale_K60-KL26_NRF)，使用NRF进行通讯**

上位机与下位机如何配置、使用，请参考：[智能车调试平台使用手册.pdf](./智能车调试平台使用手册.pdf)
