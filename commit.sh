#!/usr/bin/env bash
# 双向提交脚本：提交全部改动，并同时推送到 GitHub(origin) 与 Gitee(gitee)
# 用法：./commit.sh "提交信息"
set -euo pipefail
cd "$(dirname "$0")"

if [ $# -eq 0 ]; then
  echo "用法: ./commit.sh \"提交信息\"" >&2
  exit 1
fi
MSG="$*"
BRANCH=$(git rev-parse --abbrev-ref HEAD)

git add -A

if git diff --cached --quiet; then
  echo "没有可提交的改动"
  exit 0
fi

git commit -m "$MSG"

# 推送指定远端，失败自动重试一次（Gitee 偶发连接重置）
push_remote() {
  local remote=$1
  local try
  for try in 1 2; do
    if git push "$remote" "$BRANCH"; then
      echo "[OK] $remote/$BRANCH 推送成功"
      return 0
    fi
    echo "[RETRY] 第 $try 次推送 $remote 失败" >&2
  done
  return 1
}

FAILED=""
push_remote origin || FAILED="$FAILED origin"
push_remote gitee || FAILED="$FAILED gitee"

if [ -n "$FAILED" ]; then
  echo "以下远端推送失败，请手动重试:$FAILED" >&2
  exit 1
fi

echo "双向提交完成"
