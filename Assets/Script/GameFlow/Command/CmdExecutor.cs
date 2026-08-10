using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;

namespace Script.GameFlow.Command
{
    /// <summary>
    /// 명령 실행 파사드.
    ///
    /// 호출자는 <see cref="AddCommand"/> 와 <see cref="ExecuteCommand"/> 두 개만 알면 되고,
    /// 그 뒤의 네 가지는 이 클래스가 감춘다.
    ///   1. 우선순위 큐 (<see cref="CmdPriorityQueue"/> — 이진 힙)
    ///   2. 비동기 실행 펌프 (<see cref="ExecuteCommandAsync"/>)
    ///   3. 완료 통지 (<see cref="UniTaskCompletionSource{T}"/>)
    ///   4. 명령 단위 예외 격리
    ///
    /// <see cref="ExecuteCommand"/> 는 동기로 호출되고 즉시 반환한다.
    /// 기다릴 쪽은 <c>await executor.ExecuteCommand().Task</c> 로 받는다.
    /// </summary>
    public class CmdExecutor
    {
        private readonly CmdPriorityQueue commands;

        /// <param name="comparer">
        /// 실행 순서를 정하는 비교자. 외부에서 주입한다.
        /// null 이면 우선순위를 두지 않고 넣은 순서대로 실행한다.
        /// </param>
        public CmdExecutor(IComparer<ICmdBase> comparer = null)
        {
            commands = new CmdPriorityQueue(comparer);
        }

        public int Count => commands.Count;

        /// <summary>실행 대기열에 명령을 쌓는다. 실행 순서는 생성자에 준 비교자가 정한다.</summary>
        public void AddCommand(ICmdBase command)
        {
            if (command == null)
            {
                Debug.LogWarning("[CmdExecutor] null 명령은 넣지 않는다.");
                return;
            }

            commands.Enqueue(command);
        }

        public void Clear()
        {
            commands.Clear();
        }

        /// <summary>
        /// 쌓인 명령을 우선순위대로 실행한다. 동기 호출이며 즉시 반환한다.
        /// </summary>
        /// <param name="completeAction">모든 명령의 대기가 끝난 뒤 호출된다.</param>
        /// <returns>전부 성공했으면 true, 하나라도 실패했으면 false 가 설정되는 완료원.</returns>
        public UniTaskCompletionSource<bool> ExecuteCommand(UnityAction completeAction = null)
        {
            UniTaskCompletionSource<bool> completion = new UniTaskCompletionSource<bool>();

            ExecuteCommandAsync(completion, completeAction).Forget();

            return completion;
        }

        /// <summary>
        /// 실제 실행부. 한 명령이 실패해도 나머지를 계속 실행한다 —
        /// 로딩 도중 하나 때문에 전체가 멈추면 원인 파악이 더 어려워진다.
        /// </summary>
        private async UniTask ExecuteCommandAsync(UniTaskCompletionSource<bool> completion, UnityAction completeAction)
        {
            bool succeeded = true;

            while (commands.Count > 0)
            {
                ICmdBase command = commands.Dequeue();

                try
                {
                    await command.Execute();
                }
                catch (Exception exception)
                {
                    succeeded = false;
                    Debug.LogError($"[CmdExecutor] {command.GetType().Name} 실행 실패 : {exception}");
                }
            }

            completion.TrySetResult(succeeded);
            completeAction?.Invoke();
        }
    }
}
