﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Animator))]

public class IKControl : MonoBehaviour {
	protected Animator animator;

	public Transform rightHandObj = null;   
	public Transform leftHandObj = null;   
	public Transform rightFootObj = null;   
	public Transform leftFootObj = null;   
	public Transform lookObj = null;

	public bool ikActive = true;

	void Start () {
		animator = GetComponent<Animator>();
	}
	void OnAnimatorIK(){
		if (animator) {
			if (ikActive) {
				if (lookObj != null) {
					animator.SetLookAtWeight (1);
					animator.SetLookAtPosition (lookObj.position);             
				}    
				if (rightHandObj != null) {
					animator.SetIKPositionWeight (AvatarIKGoal.RightHand, 1);
					animator.SetIKRotationWeight (AvatarIKGoal.RightHand, 1);           
					animator.SetIKPosition (AvatarIKGoal.RightHand, rightHandObj.position);
					animator.SetIKRotation (AvatarIKGoal.RightHand, rightHandObj.rotation);           
				}        
				if (leftHandObj != null) {
					animator.SetIKPositionWeight (AvatarIKGoal.LeftHand, 1);
					animator.SetIKRotationWeight (AvatarIKGoal.LeftHand, 1);           
					animator.SetIKPosition (AvatarIKGoal.LeftHand, leftHandObj.position);
					animator.SetIKRotation (AvatarIKGoal.LeftHand, leftHandObj.rotation);           
				}        
				if (leftFootObj != null) {
					animator.SetIKPositionWeight (AvatarIKGoal.LeftFoot, 1);
					animator.SetIKRotationWeight (AvatarIKGoal.LeftFoot, 1);           
					animator.SetIKPosition (AvatarIKGoal.LeftFoot, leftFootObj.position);
					animator.SetIKRotation (AvatarIKGoal.LeftFoot, leftFootObj.rotation);           
				}        
				if (rightFootObj != null) {
					animator.SetIKPositionWeight (AvatarIKGoal.RightFoot, 1);
					animator.SetIKRotationWeight (AvatarIKGoal.RightFoot, 1);           
					animator.SetIKPosition (AvatarIKGoal.RightFoot, rightFootObj.position);
					animator.SetIKRotation (AvatarIKGoal.RightFoot, rightFootObj.rotation);           
				}        
			} else {
				animator.SetIKRotationWeight (AvatarIKGoal.RightHand, 0);       
				animator.SetIKPositionWeight (AvatarIKGoal.RightHand, 0);
				animator.SetIKRotationWeight (AvatarIKGoal.RightFoot, 0);       
				animator.SetIKPositionWeight (AvatarIKGoal.RightFoot, 0);
				animator.SetIKRotationWeight (AvatarIKGoal.LeftFoot, 0);       
				animator.SetIKPositionWeight (AvatarIKGoal.LeftFoot, 0);
				animator.SetIKRotationWeight (AvatarIKGoal.LeftHand, 0);       
				animator.SetIKPositionWeight (AvatarIKGoal.LeftHand, 0);
				animator.SetLookAtWeight (0);
			}      
		} else {
		}
	}
} 