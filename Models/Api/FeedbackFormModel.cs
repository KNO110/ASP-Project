namespace ASP_P15.Models.Api
{
public class FeedbackFormModel
{
    public Guid? EditId { get; set; }  // Используется для редактирования существующего отзыва
    public Guid? ProductId { get; set; }  // ID продукта
    public Guid? UserId { get; set; }  // ID пользователя
    public string Text { get; set; }  // Текст отзыва
    public int Rate { get; set; }  // Оценка
}
}
