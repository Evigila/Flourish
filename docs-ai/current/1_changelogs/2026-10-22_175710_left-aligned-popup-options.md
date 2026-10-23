# 浮层内容左对齐与原生选择菜单修正

时间：2026-10-22 17:57:10，America/Sao_Paulo（UTC−03:00），对应 2026-10-22T20:57:10Z。

- 按用户新规则，Flourish 所有浮层的选项内容恒左对齐，浮层位置继续依据按钮左右位置及上下可用空间自适应。
- 修正原生 option 选择标记只有横向留位的外观；使用标准 Checkbox 尺寸、固定列与选中状态。共享脚本在打开前完成实际内容离屏测量，避免依赖浮层打开过程中的固有估宽；不在打开后调整宽度。
- 生成并消费本地 Flourish `1.4.3-preview.portals.11`，六包与实际消费资源核验通过；168个包/版本组合仅五个Flourish版本变化，没有新增外部依赖或宿主样式覆盖。
- 两仓 Release 编译均0警告/错误；Flourish Framework460、Culture27、Gallery8557、Node169、预览隔离12通过；Colligere完整Web2328通过、135条件PostgreSQL跳过、0失败。
- 首帧视觉效果保留用户手动验收，不使用Computer Use；未重启Aspire、重建数据库或公开发布。两仓文档已更新，审计状态不变。

诱因、测量边界、初跑夹具修正、包证据与手工步骤见[修复报告](../1_bugreports/2026-10-22_popup-left-alignment-and-native-picker-width.md)。
