using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class ChunkedListTests
{
    [Test]
    public void TestEmptyListCount()
    {
        ChunkedList<string> m_chunkedList = new ChunkedList<string>(60f, 5);

        Assert.AreEqual(0, m_chunkedList.Count);
    }

    [Test]
    public void Test1ElementCount()
    {
        ChunkedList<string> m_chunkedList = new ChunkedList<string>(60f, 5);

        m_chunkedList.Add(0f, "Test");

        Assert.AreEqual(1, m_chunkedList.Count);
    }

    [Test]
    public void Test1ElementClearCount()
    {
        ChunkedList<string> m_chunkedList = new ChunkedList<string>(60f, 5);

        m_chunkedList.Add(0f, "Test");
        m_chunkedList.Clear();

        Assert.AreEqual(0, m_chunkedList.Count);
    }

    [Test]
    public void Test1ElementClearRe_AddThenReadCount()
    {
        ChunkedList<string> m_chunkedList = new ChunkedList<string>(60f, 5);

        m_chunkedList.Add(0f, "Test");
        m_chunkedList.Clear();
        m_chunkedList.Add(0f, "Test");

        Assert.AreEqual(1, m_chunkedList.Count);
    }

    [Test]
    public void Test1ElementValue()
    {
        ChunkedList<string> m_chunkedList = new ChunkedList<string>(60f, 5);

        m_chunkedList.Add(0f, "Test");

        Assert.AreEqual("Test", m_chunkedList[0]);
    }

    [Test]
    public void Test2ElementValue()
    {
        ChunkedList<string> m_chunkedList = new ChunkedList<string>(60f, 5);

        m_chunkedList.Add(1f, "Two");
        m_chunkedList.Add(0f, "Test");

        Assert.AreEqual("Test", m_chunkedList[0]);
        Assert.AreEqual("Two", m_chunkedList[1]);

        Assert.AreEqual(m_chunkedList.GetChunkIdOffset(0), m_chunkedList.GetChunkIdOffset(1));
    }

    [Test]
    public void Test2Element2ChunksValue()
    {
        ChunkedList<string> m_chunkedList = new ChunkedList<string>(1f, 5);

        m_chunkedList.Add(1f, "Two");
        m_chunkedList.Add(0f, "Test");

        Assert.AreEqual("Test", m_chunkedList[0]);
        Assert.AreEqual("Two", m_chunkedList[1]);

        Assert.AreNotEqual(m_chunkedList.GetChunkIdOffset(0), m_chunkedList.GetChunkIdOffset(1));
    }

    [Test]
    public void Test2Element3ChunksValue()
    {
        ChunkedList<string> m_chunkedList = new ChunkedList<string>(1f, 5);

        m_chunkedList.Add(2f, "Two");
        m_chunkedList.Add(0f, "Test");

        Assert.AreEqual("Test", m_chunkedList[0]);
        Assert.AreEqual("Two", m_chunkedList[1]);
        Assert.AreEqual(m_chunkedList.GetChunkIdOffset(1).Item1, 2);
    }

    [Test]
    public void TestDecayedChunks()
    {
        ChunkedList<string> m_chunkedList = new ChunkedList<string>(1f, 3);

        m_chunkedList.Add(0f, "0");
        m_chunkedList.Add(1f, "1");
        m_chunkedList.Add(2f, "2");
        m_chunkedList.Add(3f, "3");

        Assert.AreEqual("1", m_chunkedList[0]);
    }

    [Test]
    public void TestDecayedChunks2()
    {
        ChunkedList<string> m_chunkedList = new ChunkedList<string>(1f, 4);

        m_chunkedList.Add(0f, "0");
        m_chunkedList.Add(1f, "1");
        m_chunkedList.Add(2f, "2");
        m_chunkedList.Add(3f, "3");
        m_chunkedList.Add(4f, "4");

        Assert.AreEqual("1", m_chunkedList[0]);
        Assert.AreEqual("4", m_chunkedList[-1]);
    }

    [Test]
    public void TestDecayedChunks3()
    {
        ChunkedList<string> m_chunkedList = new ChunkedList<string>(60f, 1);

        m_chunkedList.Add(0f, "0");
        m_chunkedList.Add(60f, "4");

        Assert.AreEqual("4", m_chunkedList[0]);
        Assert.AreEqual(1, m_chunkedList.Count);
    }

    [Test]
    public void TestPerformanceOneMinute4X()
    {
        ChunkedList<string> m_chunkedList = new ChunkedList<string>(60f, 1);

        string last = "";

        for (float timer = 0f; timer < 60f * 4f; timer += 0.016f)
        {
            last = timer.ToString();
            m_chunkedList.Add(timer, last);
        }

        Assert.AreEqual(last, m_chunkedList[-1]);
    }
    
    [Test]
    public void TestPerformanceOneMinute40X()
    {
        ChunkedList<string> m_chunkedList = new ChunkedList<string>(60f, 1);

        string last = "";

        for (float timer = 0f; timer < 60f * 40f; timer += 0.016f)
        {
            last = timer.ToString();
            m_chunkedList.Add(timer, last);
        }

        Assert.AreEqual(last, m_chunkedList[-1]);
    }
}
