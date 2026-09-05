# 本地配置

将 `ARGuidingSecrets.example.json` 复制为：

```text
Assets/ARGuiding/Resources/ARGuidingSecrets.json
```

然后把 `amapWebServiceKey` 替换为受限的高德 Web 服务 Key。实际密钥文件及其 `.meta` 已被 `.gitignore` 排除，不应提交到仓库。

旧密钥已经进入 Git 历史，必须在高德控制台轮换；仅从当前源码删除不能使旧密钥重新保密。客户端密钥也应限制可用应用、签名、接口和调用额度。
