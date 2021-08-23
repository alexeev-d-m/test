using Confluent.Kafka;
using System;
using System.Net;
using System.Threading.Tasks;

namespace Common.DAL
{
  /// <summary>
  /// Управляющий для взаимодействия с Kafka
  /// </summary>
  public class KafkaManager
  {
    /// <summary>
    /// Адрес сервера для доступа к Kafka
    /// </summary>
    private const string _BOOTSTRAP_SERVER = "localhost:9092";

    /// <summary>
    /// Отправка данных в Kafka
    /// </summary>
    /// <param name="topic">Топик, куда отправляется сообщение</param>
    /// <param name="message">Отправляемое сообщение</param>
    public async Task SendAsync(string topic, string message)
    {
      ProducerConfig config = new()
      {
        BootstrapServers = _BOOTSTRAP_SERVER,
        ClientId = Dns.GetHostName()
      };

      using IProducer<Null, string> producer = new ProducerBuilder<Null, string>(config).Build();

      // Ничего с результатом делать не надо - оставил на случай, если понадобится расширить возможности программы
      // (также полезно при отладке)
#if DEBUG
      DeliveryResult<Null, string> sendResult =
#endif
        await producer.ProduceAsync(topic, new Message<Null, string> { Value = message });
    }

    /// <summary>
    /// Чтение данных из Kafka
    /// </summary>
    /// <param name="topic">Топик, из которого надо считать сообщение</param>
    /// <returns>Сообщение, прочитанное из Kafka</returns>
    public async Task<string> ReadSingleMessageAsync(string topic)
    {
      await Task.Yield();

      string result = default;
      ConsumerConfig config = new()
      {
        BootstrapServers = _BOOTSTRAP_SERVER,
        GroupId = nameof(ReadSingleMessageAsync),
        AutoOffsetReset = AutoOffsetReset.Earliest
      };

      using IConsumer<Ignore, string> consumer = new ConsumerBuilder<Ignore, string>(config).Build();

      consumer.Subscribe(topic);

      //  while (!cancelled)
      {
        ConsumeResult<Ignore, string> consumeResult = consumer.Consume(timeout: TimeSpan.FromSeconds(3));

      }

      consumer.Unsubscribe();

      return result;
    }
  }
}
