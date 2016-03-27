
using UnityEngine;
using System.Collections;

/**
 * This class is used with the movies that are shown after a rock paper scissors game.
 * Each MovieTexure from the game scene is stored in an array called materials. From 
 * this array we can access each of the different movie cases and then play which ever 
 * MovieTexture is wanted. When a movie plays, it is first moved forward .5 units so 
 * that it is front of the other MovieTextures. When it is done it should be moved back
 * .5 units as well. 
 *
 * NOTE: The naming convention for this class is {human move}{roobot move} 
 *      e.g. 
 *       The MovieTexture 'paperRock' corresponds to the movie that plays when the
 *       human played a 'paper' and the robot played a 'rock'.
 *
 *       The method 'playPaperScissors()' will play the movie for when the human plays
 *       'paper' and the robot plays 'scissors'
 */
public class Movie : MonoBehaviour
{
    public MovieTexture paperRock;
    public MovieTexture paperScissors;
    public MovieTexture rockPaper;
    public MovieTexture rockScissors;
    public MovieTexture scissorsRock;
    public MovieTexture scissorsPaper;
    public MovieTexture tie;

    private Material[] materials;

    /**
     * Runs when the game first starts. Each of the MovieTextures are set to the movies
     * in the game scene.
     */
    void Start()
    {
        materials = new Material[7];
        paperRock = materials[0].mainTexture as MovieTexture;
        paperScissors = materials[1].mainTexture as MovieTexture;
        rockPaper = materials[2].mainTexture as MovieTexture;
        rockScissors = materials[3].mainTexture as MovieTexture;
        scissorsRock = materials[4].mainTexture as MovieTexture;
        scissorsPaper = materials[5].mainTexture as MovieTexture;
        tie = materials[6].mainTexture as MovieTexture;
    }

    /**
     * Plays when the human does 'paper' and the robot does 'rock'
     */
    public void playPaperRock()
    {
        transform.GetChild(0).transform.Translate(0, .5f, 0);
        paperRock.Play();
    }

    /**
     * Plays when the human does 'paper' and the robot does 'scissors'
     */
    public void playPaperScissors()
    {
        transform.GetChild(1).transform.Translate(0, .5f, 0);
        paperScissors.Play();
    }

    /**
     * Plays when the human does 'rock' and the robot does 'paper'
     */
    public void playRockPaper()
    {
        transform.GetChild(2).transform.Translate(0, .5f, 0);
        rockPaper.Play();
    }

    /**
     * Plays when the human does 'rock' and the robot does 'scissors'
     */
    public void playRockScissors()
    {
        transform.GetChild(3).transform.Translate(0, .5f, 0);
        rockScissors.Play();
    }

    /**
     * Plays when the human does 'scissors' and the robot does 'rock'
     */
    public void playScissorsRock()
    {
        transform.GetChild(4).transform.Translate(0, .5f, 0);
        scissorsRock.Play();
    }

    /**
     * Plays when the human does 'scissors' and the robot does 'paper'
     */
    public void playScissorsPaper()
    {
        transform.GetChild(5).transform.Translate(0, .5f, 0);
        scissorsPaper.Play();
    }

    /**
     * Plays when there is a tie
     */
    public void playTie()
    {
        transform.GetChild(6).transform.Translate(0, .5f, 0);
        tie.Play();
    }

    /**
     * Used to move the movie screen into the game scene
     */
    public void moveMovieIn()
    {
        transform.Translate(15, 0, 0);
    }

    /**
     * Used to move the movie screen back out of the game scene one the movie is done playing
     */
    public void moveMovieOut()
    {
        transform.Translate(-15, 0, 0);
    }
}