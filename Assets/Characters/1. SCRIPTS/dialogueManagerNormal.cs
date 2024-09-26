using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class dialogueManagerNormal : MonoBehaviour
{
	[Header("GENERAL REFERENCES")]
	public TMP_Text dialogueTextNORMAL;
	public GameObject dialogueTextObject;
	public PlayerMovement player;
	public Transform playerTransf;
	public Animator box;
	public GameObject nextSentenceButton;

	[Header("TEXTBOX PORTRAITS")]
	public GameObject[] allFaces;

	[Header("QUEUES DON'T TOUCH")]
	public Queue<string> sentences;
	public Queue<string> faces;
	public Queue<string> textsounds;
	public Queue<dialogueEntry.textSpeedEnum> textSpeeds;

	[Header("OTHERS")]
	private bool isWritting = false;
	private bool skipwritting = false;
	private bool mustskip = false;

	public bool isShowing = false;
	private Animator anim;
	public bool isNpc = false;
	private Quaternion originalNPCRotation;
	private Transform npcTransform;

    void Start()
    {
        sentences = new Queue<string>(); 
		faces = new Queue<string>();
		textsounds = new Queue<string>();
		textSpeeds = new Queue<dialogueEntry.textSpeedEnum>();
		nextSentenceButton.SetActive(false);
    }

    void Update()
    {
		if (dialogueTextObject.activeSelf)
		{
			if ((Input.GetKey(KeyCode.Z) || Input.GetKey(KeyCode.JoystickButton1)) && !isWritting)
			{
				DisplayNextSentence();
			}

			if (Input.GetKeyDown(KeyCode.LeftShift) || Input.GetKeyDown(KeyCode.JoystickButton0))
			{
				skipwritting = true;
			}
		}
    }

	public void lookAtNPC(Transform npc)
	{
		originalNPCRotation = npc.rotation;
		npcTransform = npc;

		Vector3 npcLookDirection = (playerTransf.position - npc.position).normalized;
		Vector3 playerLookDirection = (npc.position - playerTransf.position).normalized;

		// CALCULATE TARGET
		Quaternion npcTargetRotation = Quaternion.LookRotation(new Vector3(npcLookDirection.x, 0, npcLookDirection.z));
		Quaternion playerTargetRotation = Quaternion.LookRotation(new Vector3(playerLookDirection.x, 0, playerLookDirection.z));

		LeanTween.rotate(npc.gameObject, npcTargetRotation.eulerAngles, 0.5f).setEase(LeanTweenType.easeInOutSine);
		LeanTween.rotate(playerTransf.gameObject, playerTargetRotation.eulerAngles, 0.5f).setEase(LeanTweenType.easeInOutSine);
	}

	public void AnimNpc(Animator childAnimator)
	{
		anim = childAnimator;
	}

    public void StartDialogue (dialogue dialogue)
    {
		player.isTalking = true;

		if (isNpc)
			anim.SetBool("isTalking", true);

		isShowing = true;
		player.canMove = false;
		player.anim.SetFloat("Speed", 0f);
		player.rb.velocity = Vector3.zero;
    	dialogueTextObject.SetActive(true);
		skipwritting = false;

    	sentences.Clear();
		faces.Clear();
		textsounds.Clear();
		textSpeeds.Clear();

    	foreach (dialogueEntry entry in dialogue.entries)
		{
			sentences.Enqueue(entry.sentence);
        	faces.Enqueue(entry.face);
			textsounds.Enqueue(entry.textsound);
			textSpeeds.Enqueue(entry.textSpeed);
		}

    	DisplayNextSentence();
    }

    public void DisplayNextSentence ()
    {
		//FindFirstObjectByType<SAudioManager>().Play("menu_select");
		
    	if (sentences.Count == 0)
    	{
    		StartCoroutine(EndDialogue());
    		return;
    	}

		skipwritting = false;

    	string sentence = sentences.Dequeue();
		string face = faces.Dequeue();
		string textsound = textsounds.Dequeue();
		dialogueEntry.textSpeedEnum textSpeed = textSpeeds.Dequeue();
  
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
    	StartCoroutine(Typesentence(sentence, face, textsound, textSpeed));
    }

    IEnumerator Typesentence(string sentence, string face, string textsound, dialogueEntry.textSpeedEnum textSpeed)
    {
		nextSentenceButton.SetActive(false);

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

							// TEXTSPEED

							if (textSpeed == dialogueEntry.textSpeedEnum.Normal)
								yield return new WaitForSeconds(0.035f);
							else if (textSpeed == dialogueEntry.textSpeedEnum.Slow)
								yield return new WaitForSeconds(0.1f);
							else if (textSpeed == dialogueEntry.textSpeedEnum.Fast)
								yield return new WaitForSeconds(0.025f);
							else
								yield return new WaitForSeconds(0.035f);

							// MORE WAIT FOR COMMAS AND FULL STOPS


							if (letter == ',')
								yield return new WaitForSeconds(0.2f);
							if (letter == '.' || letter == '?' || letter == '!' || letter == ')')
								yield return new WaitForSeconds(0.3f);


						}

						
						
					}
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
		skipwritting = false;
		box.SetTrigger("dialogueOut");
		sentences.Clear();

		if (isNpc)
			anim.SetBool("isTalking", false);

		yield return new WaitForSeconds(0.5f);

		if (isNpc)
			LeanTween.rotate(npcTransform.gameObject, originalNPCRotation.eulerAngles, 0.5f).setEase(LeanTweenType.easeInOutSine);
			

		player.isTalking = false;
    	dialogueTextObject.SetActive(false);
		player.canMove = true;
		StopAllCoroutines();
		StartCoroutine(FrameWait());
    }

	IEnumerator FrameWait()
	{
		yield return new WaitForSeconds(0.5f);
		isShowing = false;
	}

}
