using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using RewindSystem;

public class RewindTests
{
    [Test]
    public void RewindTestNoFramesVector()
    {
        var rewinder = new Rewind<Vector3>(Vector3.Lerp);

        var frame = rewinder.GetFrame(0f);

        Assert.AreEqual(Vector3.zero, frame, "Read back an empty frame.");
    }
    
    [Test]
    public void RewindTestNoFramesFloat()
    {
        var rewinder = new Rewind<float>(Mathf.Lerp);

        var frame = rewinder.GetFrame(541956);

        Assert.AreEqual(0f, frame, "Read back an empty frame.");
    }

    [Test]
    public void RewindTestRegisterFrame0()
    {
        var rewinder = new Rewind<Vector3>(Vector3.Lerp);
        
        rewinder.RegisterFrame(Vector3.up, 0f);
        rewinder.RegisterFrame(Vector3.down, 100f);
        rewinder.RegisterFrame(Vector3.right, 5f);
        rewinder.RegisterFrame(Vector3.left, 69f);

        var frame = rewinder.GetFrame(0f);

        Assert.AreEqual(Vector3.up, frame, "Registers a vector and reads it back.");
    }

    [Test]
    public void RewindTestRegisterFrame69()
    {
        var rewinder = new Rewind<Vector3>(Vector3.Lerp);
        
        rewinder.RegisterFrame(Vector3.down, 69f);
        rewinder.RegisterFrame(Vector3.up, 0f);
        rewinder.RegisterFrame(Vector3.left, 100f);
        rewinder.RegisterFrame(Vector3.right, 5f);

        var frame = rewinder.GetFrame(69f);

        Assert.AreEqual(Vector3.down, frame, "Registers a vector and reads it back.");
    }

    [Test]
    public void RewindTestRegisterFrameLerp()
    {
        var rewinder = new Rewind<Vector3>(Vector3.Lerp);
        
        rewinder.RegisterFrame(Vector3.down, 0f);
        rewinder.RegisterFrame(Vector3.up, 1f);

        var frame = rewinder.GetFrame(0.5f);

        Assert.AreEqual(Vector3.zero, frame, "Registers a vector and reads it back.");
    }

    [Test]
    public void RewindTestRegisterFrameLerpComplex()
    {
        var rewinder = new Rewind<Vector3>(Vector3.Lerp);
        
        rewinder.RegisterFrame(Vector3.down, 0f);
        rewinder.RegisterFrame(Vector3.up, 1f);

        var frame = rewinder.GetFrame(0.9f);

        Assert.AreEqual(Vector3.Lerp(Vector3.down, Vector3.up, 0.9f), frame, "Registers a vector and reads it back.");
    }

    [Test]
    public void RewindTestRegisterFrameLerpReverseComplex()
    {
        var rewinder = new Rewind<Vector3>(Vector3.Lerp);
        
        rewinder.RegisterFrame(Vector3.down, 1f);
        rewinder.RegisterFrame(Vector3.up, 0f);

        var frame = rewinder.GetFrame(0.9f);

        Assert.AreEqual(Vector3.Lerp(Vector3.up, Vector3.down, 0.9f), frame, "Registers a vector and reads it back.");
    }

    [Test]
    public void RewindTestFloatLerp()
    {
        var rewinder = new Rewind<float>(Mathf.Lerp);
        
        rewinder.RegisterFrame(0f, 0f);
        rewinder.RegisterFrame(100f, 1f);
        rewinder.RegisterFrame(200f, 2f);

        var frame = rewinder.GetFrame(0.9f);

        Assert.AreEqual(90f, frame, "Registers a vector and reads it back.");
    }

    [Test]
    public void RewindTestFloatEdgeCase()
    {
        var rewinder = new Rewind<float>(Mathf.Lerp);
        
        rewinder.RegisterFrame(0f, 0f);
        rewinder.RegisterFrame(100f, 1f);
        rewinder.RegisterFrame(200f, 2f);

        var frame = rewinder.GetFrame(-100f);

        Assert.AreEqual(0f, frame, "Registers a vector and reads it back.");
    }

    [Test]
    public void RewindTestFloatEdgeCase2()
    {
        var rewinder = new Rewind<float>(Mathf.Lerp);
        
        rewinder.RegisterFrame(0f, 0f);
        rewinder.RegisterFrame(100f, 1f);
        rewinder.RegisterFrame(200f, 2f);

        var frame = rewinder.GetFrame(3000f);

        Assert.AreEqual(200f, frame, "Registers a vector and reads it back.");
    }

    [Test]
    public void Rewind10000000Elements()
    {
        var rewinder = new Rewind<float>(Mathf.Lerp);
        
        for (int i = 0; i < 10000000; ++i)
        {
            rewinder.RegisterFrame(i, i / 100f);
        }

        var frame = rewinder.GetFrame(99720.5f);

        Assert.AreEqual(9972050f, frame, "Registers a vector and reads it back.");
    }
}
