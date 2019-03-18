using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    public enum state { DEFAULT, ATTACK, HIT };

    public Character characterScript;
    public float maxHp;

    public state enemyState;

    public float hitShieldDuration;

    public float attackDuration;
    public float attackHitWaitDuration;
    public float attackRange;
    public float attackFrequency;
    public float attackCooldownDuration;

    public float hitDuration;

    public float movementSpeed;

    public float targetLookDampening;

    public Transform playerTransform;

    public SphereCollider hitCollider;

    public Animator modelAnimator;

    [HideInInspector]
    public float hp;

    [HideInInspector]
    public float damageToTake;

    bool hitShieldActive;

    bool stateCycleActive;
    public bool attackCycleActive;

    bool canAttack = true;

    bool playerIsInRange;

    void Start()
    {
        characterScript.targetList.Add(this.gameObject);
        hp = maxHp;
    }

    void Update()
    {
        RunState();
    }

    void RunState()
    {
        switch (enemyState)
        {
            case state.DEFAULT:
                SwitchState_Default();
                TakeDamage();
                CanDie();
                Move();
                LookAt();
                Animate();
                break;

            case state.ATTACK:
                Attack();
                Animate();
                break;

            case state.HIT:
                Animate();
                break;
        }
    }

    void SwitchState_Default()
    {
        int rand = Mathf.RoundToInt(Random.Range(0f, attackFrequency));

        if (playerIsInRange && rand == 0 && !stateCycleActive && canAttack)
        {
            StartCoroutine(StateCycle(attackDuration, state.ATTACK, state.DEFAULT));
            StartCoroutine(AttackCooldownCycle(attackCooldownDuration + attackDuration));
        }
    }

    void TakeDamage()
    {
        if (damageToTake > 0f && !hitShieldActive)
        {
            hp -= damageToTake;

            StartCoroutine(StateCycle(hitDuration, state.HIT, state.DEFAULT));
            StartCoroutine(HitShieldCycle(hitShieldDuration + hitDuration));
        }
        else
            damageToTake = 0f;
    }

    void CanDie()
    {
        if (hp <= 0f)
        {
            characterScript.targetList.Remove(this.gameObject);

            if (characterScript.targetList.Count >= characterScript.targetIndex + 2)
                characterScript.targetIndex++;
            else
                characterScript.targetIndex = 0;

            Destroy(this.gameObject);
        }
    }

    void Attack()
    {
        if (!attackCycleActive)
            StartCoroutine(AttackCycle(attackHitWaitDuration, attackDuration - attackHitWaitDuration));
    }

    void Move()
    {
        float dist = Vector3.Distance(transform.position, playerTransform.position);

        playerIsInRange = (dist <= attackRange);

        if (!playerIsInRange)
            transform.position += transform.forward * movementSpeed * Time.deltaTime;
    }

    void LookAt()
    {
        var lookPos = playerTransform.position - transform.position;
        lookPos.y = 0f;

        var rotation = Quaternion.LookRotation(lookPos);
        transform.rotation = Quaternion.Slerp(transform.rotation, rotation, Time.deltaTime * targetLookDampening);
    }

    void Animate()
    {
        switch (enemyState)
        {
            case state.DEFAULT:
                modelAnimator.SetBool("Walk", !playerIsInRange);
                modelAnimator.SetBool("Attack", false);
                modelAnimator.SetBool("Hit", false);
                break;
            
            case state.ATTACK:
                modelAnimator.SetBool("Walk", false);
                modelAnimator.SetBool("Attack", true);
                modelAnimator.SetBool("Hit", false);
                break;
            
            case state.HIT:
                modelAnimator.SetBool("Walk", false);
                modelAnimator.SetBool("Attack", false);
                modelAnimator.SetBool("Hit", true);
                break;
        }
    }

    IEnumerator HitShieldCycle(float duration)
    {
        hitShieldActive = true;

        yield return new WaitForSeconds(duration);

        hitShieldActive = false;
    }

    IEnumerator StateCycle(float duration, state targetState, state returnState)
    {
        stateCycleActive = true;

        enemyState = targetState;

        yield return new WaitForSeconds(duration);

        enemyState = returnState;

        stateCycleActive = false;
    }

    IEnumerator AttackCycle(float wait, float duration)
    {
        attackCycleActive = true;

        yield return new WaitForSeconds(wait);

        hitCollider.enabled = true;

        yield return new WaitForSeconds(duration);

        hitCollider.enabled = false;
        attackCycleActive = false;
    }

    IEnumerator AttackCooldownCycle(float duration)
    {
        canAttack = false;

        yield return new WaitForSeconds(duration);

        canAttack = true;
    }
}
