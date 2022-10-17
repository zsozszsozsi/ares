using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Factory : Placeable
{
    public int moneyPerTick;
    public float tickInterval;

    protected override void Start()
    {
        base.Start();
        InvokeRepeating("MoneyMakingTick", 0, tickInterval);
    }

    private void MoneyMakingTick()
	{
        PlayerController.Instance.GainMoney(moneyPerTick);
	}
}
