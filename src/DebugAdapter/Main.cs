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

    // 
    // This gets run as a new thread by the UI (WaitingUI.cs)
    public static void DebugSessionLoop() {
      try {
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
