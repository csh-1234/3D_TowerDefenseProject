using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuffField : MonoBehaviour
{
    [Header("버프 효과")]
    public float damageMultiplier = 2f;
    public float rangeMultiplier = 2f;

    [Header("감지 설정")]
    public float checkInterval = 1f;
    public float checkHeight = 2f;
    public float heightOffset = 0.1f;
    public Vector3 checkSize = new Vector3(.4f, 2f, .4f);
    public LayerMask checkLayers;

    private Tower currentBuffedTower;
    private float originalHeight;
    private bool isChecking = false;
    private Vector3 lastPosition;
    private Collider[] hitColliders = new Collider[5];

    private void Start()
    {
        originalHeight = transform.position.y;
        lastPosition = transform.position;
        checkLayers = LayerMask.GetMask("Unwalkable");
        StartCoroutine(CheckForTowerAndBlock());
    }

    private IEnumerator CheckForTowerAndBlock()
    {
        WaitForSeconds wait = new WaitForSeconds(checkInterval);
        
        while (true)
        {
            if (!isChecking)
            {
                isChecking = true;
                lastPosition = transform.position;

                Vector3 checkCenter = new Vector3(transform.position.x, originalHeight + checkHeight * 0.5f, transform.position.z);
                int hitCount = Physics.OverlapBoxNonAlloc(checkCenter,checkSize * 0.5f,hitColliders,Quaternion.identity,checkLayers);

                float blockHeight = originalHeight;
                for (int i = 0; i < hitCount; i++)
                {
                    if (hitColliders[i] != null)
                    {
                        float colliderTop = hitColliders[i].bounds.max.y;
                        if (colliderTop > blockHeight)
                        {
                            blockHeight = colliderTop;
                        }
                    }
                }

                transform.position = new Vector3(transform.position.x, blockHeight + heightOffset,transform.position.z);

                Collider[] towerColliders = Physics.OverlapSphere(transform.position, checkSize.x/2);
                Tower newTower = null;

                foreach (var collider in towerColliders)
                {
                    Tower tower = collider.GetComponentInParent<Tower>();
                    if (tower != null && !tower.isPreview)
                    {
                        newTower = tower;
                        break;
                    }
                }

                // 타워 버프 상태 업데이트
                if (newTower != currentBuffedTower)
                {
                    if (currentBuffedTower != null)
                    {
                        RemoveBuff(currentBuffedTower);
                    }
                    if (newTower != null)
                    {
                        currentBuffedTower = newTower;
                        ApplyBuff(currentBuffedTower);
                    }
                    else
                    {
                        currentBuffedTower = null;
                    }
                }

                isChecking = false;
            }

            yield return wait;
        }
    }

    private void ApplyBuff(Tower tower)
    {
        if (tower != null)
        {
            tower.ApplyBuff(this);
        }
    }

    private void RemoveBuff(Tower tower)
    {
        if (tower != null)
        {
            tower.RemoveBuff(this);
        }
    }
    private void OnDestroy()
    {
        if (currentBuffedTower != null)
        {
            RemoveBuff(currentBuffedTower);
        }
    }

    #if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        if (Application.isPlaying)
        {
            // 체크 영역 표시
            Gizmos.color = Color.yellow;
            Vector3 checkCenter = new Vector3(
                transform.position.x,
                originalHeight + checkHeight * 0.5f,
                transform.position.z
            );
            Gizmos.DrawWireCube(checkCenter, checkSize);

            // 타워 감지 범위 표시
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(transform.position, checkSize.x/2);

            // 현재 버프 중인 타워와의 연결선 표시
            if (currentBuffedTower != null)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawLine(transform.position, currentBuffedTower.transform.position);
            }
        }
    }
    #endif
}