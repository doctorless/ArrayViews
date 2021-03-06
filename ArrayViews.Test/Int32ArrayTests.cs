﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArrayViews.Test
{
    [TestClass]
    public class Int32ArrayTests
    {
        [TestMethod]
        public void Int32Array_indexed_setter_alters_source_buffer_contents_according_to_native_endianness()
        {
            byte[] buf = new byte[] { 0x00, 0x01, 0x02, 0x03 };
            var s32arr = new Int32Array(buf, 0);
            s32arr[0] = -1430532899; // 0xAABBCCDD, stored as [0xAA, 0xBB, 0xCC, 0xDD] on big endian archs and [0xDD, 0xCC, 0xBB, 0xAA] on little endian archs.

            if (BitConverter.IsLittleEndian)
            {
                Assert.AreEqual(buf[0], 0xDD);
                Assert.AreEqual(buf[1], 0xCC);
                Assert.AreEqual(buf[2], 0xBB);
                Assert.AreEqual(buf[3], 0xAA);
            }
            else
            {
                Assert.AreEqual(buf[3], 0xDD);
                Assert.AreEqual(buf[2], 0xCC);
                Assert.AreEqual(buf[1], 0xBB);
                Assert.AreEqual(buf[0], 0xAA);
            }
        }

        [TestMethod]
        public void Int32Array_indexed_setter_alters_source_buffer_contents_according_to_set_endianness_when_specified()
        {
            byte[] buf = new byte[] { 0x00, 0x01, 0x02, 0x03 };
            var s32arr = new Int32Array(buf, 0, 0, !BitConverter.IsLittleEndian);
            s32arr[0] = -1430532899; // 0xAABBCCDD, stored as [0xAA, 0xBB, 0xCC, 0xDD] on big endian archs and [0xDD, 0xCC, 0xBB, 0xAA] on little endian archs.

            if (BitConverter.IsLittleEndian)
            {
                Assert.IsTrue(!s32arr.IsLittleEndian);
                Assert.AreEqual(buf[3], 0xDD);
                Assert.AreEqual(buf[2], 0xCC);
                Assert.AreEqual(buf[1], 0xBB);
                Assert.AreEqual(buf[0], 0xAA);
            }
            else
            {
                Assert.IsTrue(s32arr.IsLittleEndian);
                Assert.AreEqual(buf[0], 0xDD);
                Assert.AreEqual(buf[1], 0xCC);
                Assert.AreEqual(buf[2], 0xBB);
                Assert.AreEqual(buf[3], 0xAA);
            }
        }

        [TestMethod]
        public void Int32Array_indexed_setter_stores_and_retrieves_with_correct_signedness()
        {
            // In order to test all possible values for integers, we
            // need to create a buffer large enough to store them all,
            // except .NET won't allow arrays larger than Int32.MaxValue,
            // or 0x7FFFFFFF, which is only large enough to store
            // 0x1FFFFFFF 4-byte signed integers, with 3 extra bytes.
            // Creating four of these covers almost all 4-byte integers
            // starting from 0x00000000 all the way to 0xFFFFFFF7,
            // leaving a final check on values 0xFFFFFFF8-0xFFFFFFFF.
            // Unfortunately, not all machines are going to have
            // 2GB of contiguous RAM sitting around handy, and it may
            // not get fully GC'd in time for the next loop, so we'll
            // shrink the window even further still to 0x1FFFFFF,
            // which over 128 windows covers 0x00000000-0xFFFFFF7F,
            // leaving a final check on values 0xFFFFFF80-0xFFFFFFFF.
            uint windowSize = 0x1FFFFFF;

            for (uint window = 0; window < 128; window++)
            {
                byte[] buf = new byte[windowSize * 4];
                var s32winarr = new Int32Array(buf, 0, 0);

                for (var i = 0; i < windowSize; i++)
                {
                    s32winarr[i] = (int)((window * windowSize) + i);
                }

                for (uint i = (window * windowSize), j = 0; j < windowSize; i++, j++)
                {
                    Assert.AreEqual(s32winarr[(int)j], (int)i);
                }
            }

            byte[] finalbuf = new byte[0x80 * 4];
            var finals32winarr = new Int32Array(finalbuf, 0, 0);

            for (var i = 0; i < 0x80; i++)
            {
                finals32winarr[i] = (int)(((uint)128 * windowSize) + i);
            }

            for (uint i = (uint)(windowSize * 128), j = 0; j < 0x80; j++, i++)
            {
                Assert.AreEqual(finals32winarr[(int)j], (int)i);
            }
        }
    }
}
