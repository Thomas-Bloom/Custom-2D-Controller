using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeathHandler : MonoBehaviour {
    public PlayerControls player;
    public Transform[] checkPointsList;
    public Transform currentCheckpoint;
    public ParticleSystem deathParticles;

    private void Update() {
        
    }

    public void KillPlayer() {
        Instantiate(deathParticles, player.transform.position, Quaternion.identity);
    }
}
