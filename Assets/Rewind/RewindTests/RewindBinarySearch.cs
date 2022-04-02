using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using RewindSystem;

public class RewindBinarySearch
{
    [Test]
    public void RewindBinarySearchIndexMinus1()
    {
        var rewinder = new Rewind<Vector3>(Vector3.Lerp);
        
        rewinder.RegisterFrame(Vector3.down, 0f);
        rewinder.RegisterFrame(Vector3.right, 69f);
        rewinder.RegisterFrame(Vector3.left, 100f);

        int index = rewinder.BinarySearch(-10f);

        Assert.AreEqual(-1, index, "Binary Search test.");
    }

    [Test]
    public void RewindBinarySearchIndex0Last()
    {
        var rewinder = new Rewind<Vector3>(Vector3.Lerp);
        
        rewinder.RegisterFrame(Vector3.down, 0f);
        rewinder.RegisterFrame(Vector3.right, 69f);
        rewinder.RegisterFrame(Vector3.left, 100f);

        int index = rewinder.BinarySearch(1f);

        Assert.AreEqual(0, index, "Binary Search test.");
    }

    [Test]
    public void RewindBinarySearchIndex0()
    {
        var rewinder = new Rewind<Vector3>(Vector3.Lerp);
        
        rewinder.RegisterFrame(Vector3.down, 0f);
        rewinder.RegisterFrame(Vector3.right, 69f);
        rewinder.RegisterFrame(Vector3.left, 100f);

        int index = rewinder.BinarySearch(0f);

        Assert.AreEqual(0, index, "Binary Search test.");
    }

    [Test]
    public void RewindBinarySearchIndex1()
    {
        var rewinder = new Rewind<Vector3>(Vector3.Lerp);
        
        rewinder.RegisterFrame(Vector3.down, 0f);
        rewinder.RegisterFrame(Vector3.right, 69f);
        rewinder.RegisterFrame(Vector3.left, 100f);

        int index = rewinder.BinarySearch(69f);

        Assert.AreEqual(1, index, "Binary Search test.");
    }

    [Test]
    public void RewindBinarySearchIndex1Last()
    {
        var rewinder = new Rewind<Vector3>(Vector3.Lerp);
        
        rewinder.RegisterFrame(Vector3.down, 0f);
        rewinder.RegisterFrame(Vector3.right, 69f);
        rewinder.RegisterFrame(Vector3.left, 100f);

        int index = rewinder.BinarySearch(80f);

        Assert.AreEqual(1, index, "Binary Search test.");
    }

    [Test]
    public void RewindBinarySearchIndex2()
    {
        var rewinder = new Rewind<Vector3>(Vector3.Lerp);
        
        rewinder.RegisterFrame(Vector3.down, 0f);
        rewinder.RegisterFrame(Vector3.right, 69f);
        rewinder.RegisterFrame(Vector3.left, 100f);

        int index = rewinder.BinarySearch(100f);

        Assert.AreEqual(2, index, "Binary Search test.");
    }

    [Test]
    public void RewindBinarySearchIndex2Last()
    {
        var rewinder = new Rewind<Vector3>(Vector3.Lerp);
        
        rewinder.RegisterFrame(Vector3.down, 0f);
        rewinder.RegisterFrame(Vector3.right, 69f);
        rewinder.RegisterFrame(Vector3.left, 100f);

        int index = rewinder.BinarySearch(200f);

        Assert.AreEqual(2, index, "Binary Search test.");
    }

}
