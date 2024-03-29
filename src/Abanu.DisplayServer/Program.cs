﻿// This file is part of Abanu, an Operating System written in C#. Web: https://www.abanu.org
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Abanu.Kernel.Core;
using Abanu.Runtime;
using Mosa.Runtime.x86;

namespace Abanu.Kernel
{

    public static class Program
    {

        private static IFrameBuffer fb;
        private static ISurface sur;
        private static IGraphicsAdapter gfx;

        public static void Main()
        {
            ApplicationRuntime.Init();
            MessageManager.OnDispatchError = OnDispatchError;
            MessageManager.OnMessageReceived = MessageReceived;

            fb = CreateFrameBuffer();
            if (fb == null)
            {
                Console.WriteLine("No Framebuffer found");
                ApplicationRuntime.Exit(0);
            }
            sur = new FramebufferSurface(fb);
            gfx = new GraphicsAdapter(sur);
            gfx.SetSource(0x00115F9F);
            gfx.Rectangle(0, 0, sur.Width, sur.Height);
            gfx.Fill();

            SysCalls.RegisterService(SysCallTarget.Tmp_DisplayServer_CreateWindow);
            SysCalls.RegisterService(SysCallTarget.Tmp_DisplayServer_FlushWindow);
            SysCalls.SetServiceStatus(ServiceStatus.Ready);

            Console.WriteLine("DisplayServer ready");

            while (true)
            {
                SysCalls.ThreadSleep(0);
            }
        }

        public static void OnDispatchError(Exception ex)
        {
            Console.WriteLine(ex.Message);
        }

        public static void MessageReceived(in SystemMessage msg)
        {
            switch (msg.Target)
            {
                case SysCallTarget.Tmp_DisplayServer_CreateWindow:
                    CreateWindow(msg);
                    break;
                case SysCallTarget.Tmp_DisplayServer_FlushWindow:
                    FlushWindow(msg);
                    break;
            }

            MessageManager.Send(new SystemMessage(SysCallTarget.ServiceReturn));
        }

        public static unsafe FrameBuffer CreateFrameBuffer()
        {
            var targetProcId = SysCalls.GetProcessIDForCommand(SysCallTarget.GetFramebufferInfo);
            var fbInfoMem = SysCalls.RequestMessageBuffer(4096, targetProcId);
            SysCalls.GetFramebufferInfo(fbInfoMem);
            var fbPresent = (int*)fbInfoMem.Start;
            if (*fbPresent == 0)
                return null;

            var fbInfo = *(BootInfoFramebufferInfo*)(fbInfoMem.Start + 4);
            fbInfo.FbAddr = SysCalls.GetPhysicalMemory(fbInfo.FbAddr, fbInfo.RequiredMemory);
            return new FrameBuffer(ref fbInfo);
        }

        // TODO: Management
        private static Window CurrentWindow;

        private static unsafe void CreateWindow(in SystemMessage msg)
        {
            var sourceProcess = (int)msg.Arg1; // TODO: automatic sourceProcesID detect
            var resultAddr = (CreateWindowResult*)msg.Arg2;
            var width = (int)msg.Arg3;
            var height = (int)msg.Arg4;

            width = Math.Min(width, sur.Width);
            height = Math.Min(height, sur.Height);

            var pitch = width * 4;

            var size = height * pitch;

            var buf = SysCalls.RequestMessageBuffer((uint)size, sourceProcess);
            var result = new CreateWindowResult
            {
                Addr = buf.Start,
                Height = height,
                Width = width,
                Pitch = pitch,
                Depth = sur.Depth,
            };

            var clientArea = new MemorySurface(buf.Start, width, height, pitch, sur.Depth);
            CurrentWindow = new Window(clientArea);

            *resultAddr = result;
        }

        public static void FlushWindow(in SystemMessage msg)
        {
            var win = CurrentWindow;
            var clientArea = win.ClientArea;
            gfx.SetSource(clientArea, 0, 0);
            gfx.Rectangle(0, 0, clientArea.Width, clientArea.Height);
            gfx.Fill();
        }

    }

    public class Window
    {
        public ISurface ClientArea;

        public Window(ISurface clientArea)
        {
            ClientArea = clientArea;
        }
    }

}
