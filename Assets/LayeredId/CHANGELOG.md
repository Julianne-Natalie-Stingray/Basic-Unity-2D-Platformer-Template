# Changelog

## 0.1.0

- 初始版本：`ILayeredId`、`LayeredIdCatalog`、`[LayeredIdCatalog]`、`LayeredIdMember`、`LayeredIdIssue`。
- 目录从 `public const string` 成员反射生成，按 `StringComparison.Ordinal` 去重与排序。
- 编辑器：`LayeredIdDrawer`（`/` 分层下拉 + 缺失红底线）、`LayeredIdCatalogWindow`（成员与问题）。
- EditMode 测试：排序、Ordinal 大小写、空路径、重复路径、空目录。
- 运行时程序集无 UnityEngine 依赖；`autoReferenced = false`。