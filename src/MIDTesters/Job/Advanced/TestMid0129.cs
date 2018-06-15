﻿using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenProtocolInterpreter.Job.Advanced;

namespace MIDTesters.Job.Advanced
{
    [TestClass]
    public class TestMid0129 : MidTester
    {
        [TestMethod]
        public void Mid0129Revision1()
        {
            string package = "00200129            ";
            var mid = _midInterpreter.Parse(package);

            Assert.AreEqual(typeof(MID_0129), mid.GetType());
            Assert.AreEqual(package, mid.Pack());
        }

        [TestMethod]
        public void Mid0129Revision2()
        {
            string package = "00290129002         010302123";
            var mid = _midInterpreter.Parse<MID_0129>(package);

            Assert.AreEqual(typeof(MID_0129), mid.GetType());
            Assert.IsNotNull(mid.ChannelId);
            Assert.IsNotNull(mid.ParameterSetId);
            Assert.AreEqual(package, mid.Pack());
        }
    }
}