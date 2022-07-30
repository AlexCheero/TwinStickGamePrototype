using System.Collections;
using System.Collections.Generic;
using ECS;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class BitmaskTests
{
    [Test]
    public void SetCheckTest()
    {
        var bitmask = new BitMask();
        var bitsNum = Random.Range(1, 1000);
        for (int i = 0; i < bitsNum; i++)
        {
            bitmask.Set(i);
            Assert.IsTrue(bitmask.Check(i));
        }
    }

    [Test]
    public void CheckOutOfRangeTest()
    {
        var bitmask = new BitMask();
        var bitsNum = Random.Range(1, 1000);
        for (int i = 0; i < bitsNum; i++)
            bitmask.Set(i);
        for (int i = bitsNum; i < bitsNum + 1000; i++)
            Assert.IsTrue(!bitmask.Check(i));
    }

    [Test]
    public void ArrSetTest()
    {
        var itersNum = Random.Range(1, 1000);
        var bitsList = new List<int>();
        for (int i = 0; i < itersNum; i++)
        {
            var remainder = i % BitMask.SizeOfPartInBits;
            if (Random.Range(0, 2) == 1 || remainder == 0 || remainder == BitMask.SizeOfPartInBits - 1)
                bitsList.Add(i);
        }

        var bitmask = new BitMask();
        bitmask.Set(bitsList.ToArray());
        for (int i = 0; i < bitsList.Count; i++)
            Assert.IsTrue(bitmask.Check(bitsList[i]));
    }

    [Test]
    public void LengthTest()
    {
        var bitmask = new BitMask();
        var maxBit = 0;
        var bitsNum = Random.Range(1, 1000);
        for (int i = 0; i < bitsNum; i++)
        {
            var remainder = i % BitMask.SizeOfPartInBits;
            if (Random.Range(0, 2) == 1 || remainder == 0 || remainder == BitMask.SizeOfPartInBits - 1)
            {
                bitmask.Set(i);
                if (i > maxBit)
                    maxBit = i;
            }
        }

        Assert.AreEqual(maxBit + 1, bitmask.Length);
    }

    [Test]
    public void CtorTest()
    {
        var itersNum = Random.Range(1, 1000);
        var bitsList = new List<int>();
        for (int i = 0; i < itersNum; i++)
        {
            var remainder = i % BitMask.SizeOfPartInBits;
            if (Random.Range(0, 2) == 1 || remainder == 0 || remainder == BitMask.SizeOfPartInBits - 1)
                bitsList.Add(i);
        }

        var bitmask = new BitMask(bitsList.ToArray());
        for (int i = 0; i < bitsList.Count; i++)
            Assert.IsTrue(bitmask.Check(bitsList[i]));
    }

    [Test]
    public void CopyDupEqTest()
    {
        var bitmask = CreateMask(1000);

        var copy = new BitMask();
        copy.Copy(bitmask);
        Assert.IsTrue(copy.Equals(bitmask));
        Assert.IsTrue(bitmask.Equals(copy));
        Assert.AreEqual(copy.Length, bitmask.Length);
        for (int i = 0; i < copy.Length; i++)
            Assert.AreEqual(copy.Check(i), bitmask.Check(i));

        var dup = bitmask.Duplicate();
        Assert.IsTrue(dup.Equals(bitmask));
        Assert.IsTrue(bitmask.Equals(dup));
        Assert.AreEqual(dup.Length, bitmask.Length);
        for (int i = 0; i < dup.Length; i++)
            Assert.AreEqual(dup.Check(i), bitmask.Check(i));
    }

    [Test]
    public void UnsetTest()
    {
        var bitmask = CreateMask(1000);
        for (int i = 0; i < bitmask.Length; i++)
            bitmask.Unset(i);
        for (int i = 0; i < bitmask.Length; i++)
            Assert.IsTrue(!bitmask.Check(i));
    }

    [Test]
    public void UnsetLastLengthTest()
    {
        var bitmask = CreateMask(1000);
        var length = bitmask.Length;
        bitmask.Unset(length - 1);
        Assert.IsTrue(length > bitmask.Length);
    }

    [Test]
    public void ClearTest()
    {
        var bitmask = CreateMask(1000);
        bitmask.Clear();
        Assert.AreEqual(0, bitmask.Length);
        for (int i = 0; i < bitmask.Length; i++)
            Assert.IsTrue(!bitmask.Check(i));
    }

    [Test]
    public void IncludeTest()
    {
        var bitmask = CreateMask(1000);
        var includeFilter = bitmask.Duplicate();
        for (int i = 0; i < includeFilter.Length; i++)
        {
            if (Random.Range(0, 2) == 1)
                includeFilter.Unset(i);
        }

        Assert.IsTrue(bitmask.InclusivePass(includeFilter));

        for (int i = 0; true; i = (i + 1) % bitmask.Length)
        {
            if (Random.Range(0, 2) == 1 && !bitmask.Check(i))
            {
                Assert.IsTrue(!includeFilter.Check(i));
                includeFilter.Set(i);
                break;
            }
        }
        Assert.IsTrue(!bitmask.InclusivePass(includeFilter));
    }

    [Test]
    public void ExcludeTest()
    {
        var bitmask = CreateMask(1000);
        var excFilter = bitmask.Duplicate();
        for (int i = 0; i < excFilter.Length; i++)
        {
            if (excFilter.Check(i))
                excFilter.Unset(i);
            else
                excFilter.Set(i);
        }

        Assert.IsTrue(bitmask.ExclusivePass(excFilter));

        foreach (var bit in bitmask)
        {
            if (Random.Range(0, 2) == 1)
            {
                Assert.IsTrue(!excFilter.Check(bit));
                excFilter.Set(bit);
                break;
            }
        }
        Assert.IsTrue(!bitmask.ExclusivePass(excFilter));
    }
    
    private BitMask CreateMask(int maxLength)
    {
        var bitmask = new BitMask();
        var bitsNum = Random.Range(1, maxLength);
        for (int i = 0; i < bitsNum; i++)
        {
            var remainder = i % BitMask.SizeOfPartInBits;
            if (Random.Range(0, 2) == 1 || remainder == 0 || remainder == BitMask.SizeOfPartInBits - 1)
                bitmask.Set(i);
        }
        return bitmask;
    }
}
