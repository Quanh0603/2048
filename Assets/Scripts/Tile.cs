using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

using UnityEngine.UIElements;

public class Tile : MonoBehaviour
{
    
    public int _value = 2;
    
    private bool _isAnimating;

    private float _count;
    
    private GameObject _currentNumberPrefab;
    public GameObject[] _tilesNumber;
    
    [SerializeField] private TileSetting _tileSetting;

    private Tile _mergeTile;
    
    private Animator _animator;
    
    private TileManager _tileManager;
    
    private Vector3 _startPos;
    private Vector3 _endPos;
    
    // Start is called before the first frame update
    public void SetValue(int value)
    {
        _value = value;
        _currentNumberPrefab = _tilesNumber[LogBase2(value) - 1];
        Instantiate(_currentNumberPrefab, transform);  
    }

    private void Start()
    {
        _animator = GetComponent<Animator>();
        _tileManager = FindObjectOfType<TileManager>();
    }

    private void Update()
    {
        if (!_isAnimating)
        {
            return;
        }

        _count += Time.deltaTime;
        
        float t = _count / _tileSetting.animationTime;
        t = _tileSetting.animationCurve.Evaluate(t);
        Vector3 newPos = Vector3.Lerp(_startPos, _endPos, t);

        transform.position = newPos;

        if (_count >= _tileSetting.animationTime)
        {
            _isAnimating = false;
            if( _mergeTile != null )
            {
                int newScore = _value + _mergeTile._value;
                _tileManager.AddScore(newScore);
                _animator.SetTrigger("Merge");
                SetValue(newScore);
                Destroy(_mergeTile.gameObject);
                _mergeTile = null;
            }
        } 
            
    }
    private int LogBase2(int value)
    {
        return (int)(Mathf.Log(value) / Mathf.Log(2));
    }
    //bien instant de xac dinh co thuc hien animation hay khong?
    //true thi dat luon vi tri newPos cua no o do, con
    public void SetPosition(Vector3 newPos, bool instant)
    {
        if (instant)
        {
            transform.position = newPos;
            return;
        }
        _startPos = transform.position;
        _endPos = newPos;
        _isAnimating = true;
        _count = 0;
        if (_mergeTile != null)
        {
            _mergeTile.SetPosition(newPos,false);
        }
    }
    
    public bool Merge(Tile otherTile)
    {
        if (!CanMerge(otherTile))
        {
            return false;
        }
        _mergeTile = otherTile;
        return true;
    }
    public bool CanMerge(Tile otherTile)
    {
        if (otherTile == null)
        {
            return false;
        }
        if(this._value != otherTile._value)
        {
            return false;
        }
        if(_mergeTile != null || otherTile._mergeTile != null)
        {
            return false;
        }
        return true;
    }

}