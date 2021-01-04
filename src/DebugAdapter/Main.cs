// Original work by:
/*---------------------------------------------------------------------------------------------
*  Copyright (c) Microsoft Corporation. All rights reserved.
*  Licensed under the MIT License. See License.txt in the project root for license information.
*--------------------------------------------------------------------------------------------*/

// Modified by:
/*---------------------------------------------------------------------------------------------
*  Copyright (c) NEXON Korea Corporation. All rights reserved.
*  Licensed under the MIT License. See License.txt in the project root for license information.
*--------------------------------------------------------------------------------------------*/

using System;
using System.Windows.Forms;

namespace VSCodeDebug
{
  internal class Program {

    public static WaitingUI WaitingUI;

    private static void Main(string[] argv) {
      WaitingUI = new WaitingUI();

      Application.Run(WaitingUI); // Launch the UI
    }

    private static ICDPSender toVSCode;

    public static void Stop() {
      if (toVSCode != null) {
        toVSCode.SendMessage(new TerminatedEvent());
      }
    }

    public static void DebugSessionLoop() {
      try {
        // 우선 VS Code와의 통신을 조립한다.
        // 디버기와의 통신은 launch 명령어 실행 이후에 조립한다.

        // First, assemble communication with VS Code.
        // Communication with the debugger is assembled after executing the launch command.

        var debugSession = new DebugSession();

        var cdp = new VSCodeDebugProtocol(debugSession);

        debugSession.toVSCode = cdp;
        toVSCode = cdp;

        cdp.Loop(Console.OpenStandardInput(), Console.OpenStandardOutput());

      } catch (Exception e) {

         MessageBox.OK(e.ToString());
      }
    }
  } // End program class
} // End namespace
