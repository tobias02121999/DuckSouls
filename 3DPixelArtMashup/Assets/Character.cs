using System.Collections;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Character : MonoBehaviour
{
    public enum state { DEFAULT, TARGET, DASH, BLOCK, ATTACK };

    public float damage;

    public float movementSpeed;

    public float staminaRegenSpeed;

    public float dashSpeed;
    public float dashDuration;
    public float dashStaminaCost;

    public float attackHitWaitDuration;
    public float attackDuration;
    public float attackStaminaCost;

    public float blockStaminaCost;

    public Transform modelTransform;
    public Animator modelAnimator;

    public float targetLookDampening;

    public SphereCollider hitCollider;

    public Transform targetIndicatorGameobject;

    public bool doFrameCap;
    public int frameCap;

    public bool isTargeting;

    public state playerState;

    public Transform moveAngleTransform;

    public Slider staminaSlider;

    [HideInInspector]
    public List<GameObject> targetList = new List<GameObject>();

    [HideInInspector]
    public Transform targetTransform;

    [HideInInspector]
    public bool hit;

    [HideInInspector]
    public float stamina;

    int targetIndex;

    bool stateCycleActive;
    bool attackCycleActive;

    void Start()
    {
        if (doFrameCap)
        {
            QualitySettings.vSyncCount = 0;
            Application.targetFrameRate = frameCap;
        }

        targetTransform = GetComponent<Transform>();
    }

    void Update()
    {
        RunState();

        staminaSlider.value = stamina;
    }

    void RunState()
    {
        switch (playerState)
        {
            case state.DEFAULT:
                SwitchState_Default();
                Move();
                LookAt();
                Animate();
                CanDie();
                StaminaRegen();
                break;

            case state.TARGET:
                SwitchState_Target();
                Target();
                Move();
                LookAt();
                Animate();
                CanDie();
                StaminaRegen();
                break;

            case state.DASH:
                Dash();
                CanDie();
                break;

            case state.BLOCK:
                SwitchState_Block();
                LookAt();
                Animate();
                CanDie();
                StaminaReduce(blockStaminaCost);
                break;

            case state.ATTACK:
                Attack();
                Animate();
                CanDie();
                break;
        }
    }

    void SwitchState_Default()
    {
        if (Input.GetButtonDown("Target"))
        {
            targetList = targetList.OrderBy(x => Vector3.Distance(transform.position, x.transform.position)).ToList();

            isTargeting = true;
            playerState = state.TARGET;
        }

        if (Input.GetButtonDown("Dash") && !stateCycleActive)
        {
            StaminaReduce(dashStaminaCost);
            StartCoroutine(StateCycle(dashDuration, state.DASH, state.DEFAULT));
        }

        if (Input.GetButton("Block"))
            playerState = state.BLOCK;

        if (Input.GetButtonDown("Attack") && !stateCycleActive)
        {
            StaminaReduce(attackStaminaCost);
            StartCoroutine(StateCycle(attackDuration, state.ATTACK, state.DEFAULT));
        }
    }

    void SwitchState_Target()
    {
        if (Input.GetButtonDown("Target"))
        {
            if (targetList.Count >= targetIndex + 2)
                targetIndex++;
            else
                targetIndex = 0;
        }

        if (Input.GetButtonDown("Cancel Target") || targetTransform == null)
        {
            targetIndex = 0;
            isTargeting = false;

            playerState = state.DEFAULT;
        }

        if (Input.GetButtonDown("Dash") && !stateCycleActive)
        {
            StaminaReduce(dashStaminaCost);
            StartCoroutine(StateCycle(dashDuration, state.DASH, state.TARGET));
        }

        if (Input.GetButton("Block"))
            playerState = state.BLOCK;

        if (Input.GetButtonDown("Attack") && !stateCycleActive)
        {
            StaminaReduce(attackStaminaCost);
            StartCoroutine(StateCycle(attackDuration, state.ATTACK, state.TARGET));
        }
    }

    void SwitchState_Block()
    {
        if (Input.GetButtonUp("Block"))
        {
            if (!isTargeting)
                playerState = state.DEFAULT;
            else
                playerState = state.TARGET;
        }
    }

    void Move()
    {
        transform.position += transform.right * Input.GetAxis("Horizontal") * movementSpeed * Time.deltaTime;
        transform.position += transform.forward * Input.GetAxis("Vertical") * movementSpeed * Time.deltaTime;
    }

    void LookAt()
    {
        var angle = Mathf.Atan2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical")) * Mathf.Rad2Deg;

        moveAngleTransform.rotation = Quaternion.Euler(moveAngleTransform.eulerAngles.x, transform.eulerAngles.y + angle, moveAngleTransform.eulerAngles.z);

        if (!isTargeting)
        {
            if (Input.GetAxis("Horizontal") != 0f || Input.GetAxis("Vertical") != 0f)
                modelTransform.rotation = Quaternion.Euler(transform.eulerAngles.x, angle, transform.eulerAngles.z);

            var rotation = Quaternion.LookRotation(new Vector3(0f, 0f, 0f));
            transform.rotation = Quaternion.Slerp(transform.rotation, rotation, Time.deltaTime * targetLookDampening);
        }
        else
        {
            var lookPos = targetTransform.position - modelTransform.position;
            lookPos.y = 0f;

            var rotation = Quaternion.LookRotation(lookPos);
            transform.rotation = Quaternion.Slerp(transform.rotation, rotation, Time.deltaTime * targetLookDampening);
            modelTransform.rotation = transform.rotation;
        }
    }

    void Target()
    {
        targetTransform = targetList[targetIndex].transform;
        targetIndicatorGameobject.transform.position = new Vector3(targetTransform.position.x, targetIndicatorGameobject.transform.position.y, targetTransform.position.z);

        targetIndicatorGameobject.GetComponent<MeshRenderer>().enabled = isTargeting;
    }

    void Dash()
    {
        transform.position += moveAngleTransform.forward * dashSpeed * Time.deltaTime;
        modelTransform.rotation = moveAngleTransform.rotation;
    }

    void Attack()
    {
        if (!attackCycleActive)
            StartCoroutine(AttackCycle(attackHitWaitDuration, attackDuration - attackHitWaitDuration));
    }

    void CanDie()
    {
        if (hit)
        {
            if (playerState != state.BLOCK)
                SceneManager.LoadScene(SceneManager.GetActiveScene().name);
            else
                hit = false;
        }
    }

    void StaminaRegen()
    {
        if (stamina <= 1f - staminaRegenSpeed)
            stamina += staminaRegenSpeed;
        else
            stamina = 1f;
    }

    void StaminaReduce(float amount)
    {
        if (stamina >= amount)
            stamina -= amount;
        else
            stamina = 0f;
    }

    void Animate()
    {
        var isWalking = (Input.GetAxis("Horizontal") != 0f || Input.GetAxis("Vertical") != 0f);

        switch (playerState)
        {
            case state.DEFAULT:
                modelAnimator.SetBool("Walk", isWalking);
                modelAnimator.SetBool("Block", false);
                modelAnimator.SetBool("Dash", false);
                modelAnimator.SetBool("Attack", false);
                break;

            case state.TARGET:
                modelAnimator.SetBool("Walk", isWalking);
                modelAnimator.SetBool("Block", false);
                modelAnimator.SetBool("Dash", false);
                modelAnimator.SetBool("Attack", false);
                break;

            case state.DASH:
                modelAnimator.SetBool("Walk", false);
                modelAnimator.SetBool("Block", false);
                modelAnimator.SetBool("Dash", true);
                modelAnimator.SetBool("Attack", false);
                break;

            case state.BLOCK:
                modelAnimator.SetBool("Walk", false);
                modelAnimator.SetBool("Block", true);
                modelAnimator.SetBool("Dash", false);
                modelAnimator.SetBool("Attack", false);
                break;

            case state.ATTACK:
                modelAnimator.SetBool("Walk", false);
                modelAnimator.SetBool("Block", false);
                modelAnimator.SetBool("Dash", false);
                modelAnimator.SetBool("Attack", true);
                break;
        }
    }

    IEnumerator StateCycle(float duration, state targetState, state returnState)
    {
        stateCycleActive = true;

        playerState = targetState;

        yield return new WaitForSeconds(duration);

        playerState = returnState;

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
}
