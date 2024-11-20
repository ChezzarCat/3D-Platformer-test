using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Cinemachine;

public class dialogueManagerNormal : MonoBehaviour
{
	[Header("GENERAL REFERENCES")]
	public ControllerDetection controllerDetection;
	public TMP_Text dialogueTextNORMAL;
	public GameObject dialogueTextObject;
	public PlayerMovement player;
	public Transform playerTransf;
	public Animator box;
	public GameObject nextSentenceButton;
	public CinemachineFreeLook dialogueCam;
	public TMP_Text skipButton;
	public GameObject jestburstPrefab;
	public Transform transformJester;

	[Header("TEXTBOX PORTRAITS")]
	public GameObject[] allFaces;

	[Header("QUEUES DON'T TOUCH")]
	public Queue<string> sentences;
	public Queue<string> faces;
	public Queue<string> textsounds;

	[Header("OTHERS")]
	private bool isWritting = false;
	private bool skipwritting = false;
	private bool mustskip = false;

	public bool isShowing = false;
	private Animator anim;
	public bool isNpc = false;
	private Quaternion originalNPCRotation;
	private Transform npcTransform;
	private string currentLanguage;
	bool jesterHAStoleave = false;

    void Start()
    {
        sentences = new Queue<string>(); 
		faces = new Queue<string>();
		textsounds = new Queue<string>();
		nextSentenceButton.SetActive(false);
		jesterHAStoleave = false;
		dialogueCam.Priority = 1;
		currentLanguage = PlayerPrefs.GetString("GameLanguage", "ENG");

		switch (currentLanguage)
		{
			case "ENG": skipButton.text = " SKIP:"; break;
			case "ESP": skipButton.text = "SALTA:"; break;
		}
    }

    void Update()
    {
		if (dialogueTextObject.activeSelf)
		{
			if ((Input.GetKey(KeyCode.E) || Input.GetKey(controllerDetection.jump)) && !isWritting)
			{
				DisplayNextSentence();
			}

			if (Input.GetKeyDown(KeyCode.LeftShift) || Input.GetKeyDown(controllerDetection.run))
			{
				skipwritting = true;
			}
		}
    }

	/*public void ActivateRig(int rigIndex)
    {
        //
    }*/

	public void lookAtNPC(Transform npc)
	{
		originalNPCRotation = npc.rotation;
		npcTransform = npc;

		if (isNpc)
		{
			Vector3 npcLookDirection = (playerTransf.position - npc.position).normalized;
			Quaternion npcTargetRotation = Quaternion.LookRotation(new Vector3(npcLookDirection.x, 0, npcLookDirection.z));
			LeanTween.rotate(npc.gameObject, npcTargetRotation.eulerAngles, 0.5f).setEase(LeanTweenType.easeInOutSine);
		}

		Vector3 playerLookDirection = (npc.position - playerTransf.position).normalized;
		Quaternion playerTargetRotation = Quaternion.LookRotation(new Vector3(playerLookDirection.x, 0, playerLookDirection.z));
		LeanTween.rotate(playerTransf.gameObject, playerTargetRotation.eulerAngles, 0.5f).setEase(LeanTweenType.easeInOutSine);
	}

	public void AnimNpc(Animator childAnimator)
	{
		anim = childAnimator;
	}

    public void StartDialogue (dialogue dialogue)
    {
		jesterHAStoleave = false;
		dialogueCam.Priority = 20;

		if (isNpc)
			anim.SetBool("isTalking", true);
			
		isShowing = true;
		player.canMove = false;
		player.anim.SetFloat("Speed", 0f);
		player.GetRigidbody().velocity = Vector3.zero;
    	dialogueTextObject.SetActive(true);
		skipwritting = false;

    	sentences.Clear();
		faces.Clear();
		textsounds.Clear();

    	foreach (dialogueEntry entry in dialogue.entries)
		{
			sentences.Enqueue(entry.sentence);
        	faces.Enqueue(entry.face);
			textsounds.Enqueue(entry.textsound);
		}

    	DisplayNextSentence();
    }

    public void DisplayNextSentence ()
    {
		//FindFirstObjectByType<SAudioManager>().Play("menu_select");
		if (isNpc)
			anim.SetBool("isTalking", true);
		
    	if (sentences.Count == 0)
    	{
    		StartCoroutine(EndDialogue());
    		return;
    	}

		skipwritting = false;

    	string sentence = sentences.Dequeue();
		string face = faces.Dequeue();
		string textsound = textsounds.Dequeue();
  
    	foreach (GameObject hideAllFaces in allFaces)
    	{
    		hideAllFaces.SetActive(false);
    	}

		foreach (GameObject obj in allFaces)
		{
			if (obj.name == face)
			{
				obj.SetActive(true);
				break;
			}
		}

    	StopAllCoroutines();
    	StartCoroutine(Typesentence(sentence, face, textsound));
    }

    IEnumerator Typesentence(string sentence, string face, string textsound)
    {
		nextSentenceButton.SetActive(false);

		// LEAVE ------------------------------------

		if (face == "leave")
			anim.SetTrigger("Leave");

		// SHOCKED ------------------------------------

		if (face == "shocked")
		{
			anim.SetBool("isShock", true);
			anim.SetBool("isTalking", false);
		}

		// SHADOW LUNA  ------------------------------------

		if (face == "activateShadowLuna")
			StartCoroutine("ShadowLuna");

		// IGNORE TRIGGER  ------------------------------------

		if (face == "destroyignore")
		{
			GameObject obj = GameObject.Find("IGNORE GUIDER");
			if (obj != null)
			{
				Destroy(obj);
			}
			else
				Debug.Log("IGNORE GUIDER not found in the scene.");
		}

		// JESTER DISSAPEAR ------------------------------------

		if (face == "jesterLeave")
		{
			jesterHAStoleave = true;
			anim.SetTrigger("isBurst");

			if (jestburstPrefab != null)
				StartCoroutine("jesterLeaveCoroutine");
			else
				Debug.Log("JesterBurst prefab not found.");

			
		}

		// -------------------------------------------
		// NORMAL  ------------------------------------

		if (face != "wait")
		{

			isWritting = true;
			dialogueTextNORMAL.text = "";

			foreach (char letter in sentence.ToCharArray())
			{
				
				if (skipwritting)
					{
						dialogueTextNORMAL.text = sentence;
					}
					else
					{
						// TEXTMESHPRO INTERN COMMANDS HAVE THESE AND THE CODE TYPES THEM OUT UNTIL THEY ARE FILLED SO I SKIP THEM LIKE THIS
						if (letter == '>')
							mustskip = false;
						else if (letter == '<')
							mustskip = true;

						dialogueTextNORMAL.text += letter;

						if (mustskip == false)
						{
							if (letter != ' ' || letter != '<' || letter != '>')	//SKIP SOUND FOR SPACES
							{
								if (string.IsNullOrEmpty(textsound))
									FindFirstObjectByType<SAudioManager>().Play("Default Text");
								else if (textsound != "none")
									FindFirstObjectByType<SAudioManager>().Play(textsound);
							}

							yield return new WaitForSeconds(0.035f);

							// MORE WAIT FOR COMMAS AND FULL STOPS


							if (letter == ',')
								yield return new WaitForSeconds(0.2f);
							if (letter == '.' || letter == '?' || letter == '!' || letter == ')')
								yield return new WaitForSeconds(0.3f);


						}

						
						
					}
			}

			if (face == "shocked")
			{
				anim.SetBool("isShock", false);
			}

			skipwritting = false;
			isWritting = false;
			nextSentenceButton.SetActive(true);

		}
		else
		{
			float waitTime;
        
			// IN CASE OF WAIT DIALOGUE, YOU HAVE TO PUT THE TIME OF WAITING AS A FLOAT NUMBER IN THE "SENTENCE"
			if (float.TryParse(sentence, out waitTime))
			{
				dialogueTextObject.SetActive(false);
				yield return new WaitForSeconds(waitTime);
				dialogueTextObject.SetActive(true);
				DisplayNextSentence();
			}
			else
			{
				Debug.LogError("Error: Sentence provided for 'wait' is not a valid number. Sentence: " + sentence);
			}
		}
    }
    

    IEnumerator EndDialogue()
    {	
		if (jesterHAStoleave == true)
			yield return new WaitForSeconds(0.5f);
			
		skipwritting = false;
		box.SetTrigger("dialogueOut");
		sentences.Clear();
		dialogueCam.Priority = 1;

		if (isNpc)
			anim.SetBool("isTalking", false);
			

		yield return new WaitForSeconds(0.5f);

		if (isNpc)
			LeanTween.rotate(npcTransform.gameObject, originalNPCRotation.eulerAngles, 0.5f).setEase(LeanTweenType.easeInOutSine);
			

    	dialogueTextObject.SetActive(false);
		player.canMove = true;

		if (jesterHAStoleave == true)
		{
			GameObject objJESTER = GameObject.Find("JESTER NPC");
			if (objJESTER != null)
			{
				Destroy(objJESTER);
			}
			else
				Debug.Log("JESTER NPC not found in the scene.");
		}

		StopAllCoroutines();
		StartCoroutine(FrameWait());
    }

	IEnumerator FrameWait()
	{
		yield return new WaitForSeconds(0.5f);
		isShowing = false;
	}

	IEnumerator jesterLeaveCoroutine()
	{
		FindFirstObjectByType<SAudioManager>().Play("laugh_jester");
		yield return new WaitForSeconds(0.5f);
		ParticleSystem jestburst = Instantiate(jestburstPrefab, transformJester.position, transformJester.rotation).GetComponent<ParticleSystem>();
		Destroy(jestburst, 3);
	}

	IEnumerator ShadowLuna()
	{
		yield return new WaitForSeconds(1f);
		Transform parentTransform = GameObject.Find("LUNASHADOW_TRIGGER").transform;
		Transform lunaShadowTransform = parentTransform.Find("LUNASHADOW");
		Transform triggerlunaShadowTransform = parentTransform.Find("TRIGGERCAMERA");

		if (lunaShadowTransform != null && triggerlunaShadowTransform != null)
		{
			lunaShadowTransform.gameObject.SetActive(true);
			triggerlunaShadowTransform.gameObject.SetActive(true);
		}
		else
		{
			Debug.Log("LunaShadow_NOTFOUND");
		}
	}

}
