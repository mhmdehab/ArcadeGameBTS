using UnityEngine;


public class TutorialSlides : MonoBehaviour
{
    public GameObject nextButton;
    public GameObject prevButton;
    private int currentSlide = 0;
    public GameObject[] slides;
    void Start()
    {
        showSlide(0);
    }
    void showSlide(int index)
    {
        for (int i = 0; i < slides.Length; i++)
        {
            slides[i].SetActive(false);

        }
        slides[index].SetActive(true);
        currentSlide = index;
        nextButton.SetActive(currentSlide < slides.Length - 1);
        prevButton.SetActive(currentSlide > 0);
    }
    public void nextSlide()
    {
        if (currentSlide < slides.Length-1)
        {
            showSlide(currentSlide+1);
        }
    }
    public void prevSlide()
    {
        if (currentSlide > 0)    
        {
            showSlide(currentSlide-1);
        }
    }
}
