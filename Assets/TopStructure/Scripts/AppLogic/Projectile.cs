using System.Collections;
using UnityEngine;
namespace EmpireClash
{

    // 投射物
    public class Projectile : MonoBehaviour
    {
        [Header("投射物设置")]
        public float speed = 10f;
        public float lifeTime = 5f;
        public ParticleSystem hitEffect;

        private IDamageable target;
        private int damage;
        private Vector3 targetPosition;

        public void SetTarget(IDamageable target, int damage)
        {
            this.target = target;
            this.damage = damage;

            if (target is MonoBehaviour targetObj)
            {
                targetPosition = targetObj.transform.position;
            }

            // 自动销毁
            Destroy(gameObject, lifeTime);
        }

        void Update()
        {
            if (target == null)
            {
                Destroy(gameObject);
                return;
            }

            // 更新目标位置
            if (target is MonoBehaviour targetObj)
            {
                targetPosition = targetObj.transform.position;
            }

            // 移动投射物
            Vector3 direction = (targetPosition - transform.position).normalized;
            transform.position += direction * speed * Time.deltaTime;
            transform.LookAt(targetPosition);

            // 检查碰撞
            if (Vector3.Distance(transform.position, targetPosition) < 0.5f)
            {
                HitTarget();
            }
        }

        private void HitTarget()
        {
            // 播放击中特效
            if (hitEffect != null)
                Instantiate(hitEffect, transform.position, Quaternion.identity);

            // 造成伤害
            if (target != null)
            {
                target.TakeDamage(damage);
            }

            Destroy(gameObject);
        }
    }
}