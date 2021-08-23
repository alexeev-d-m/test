using Common.DAL;
using NUnit.Framework;
using System;
using System.Threading.Tasks;

namespace Tests
{
  /// <summary>
  /// ����� ��� Kafka
  /// </summary>
  public class KafkaTests
  {
    /// <summary>
    /// �������� � ��������� ���������
    /// </summary>
    [Test]
    public async Task SendAndReceiveMessage_AreEqual()
    {
      const string TEST_TOPIC = "quickstart-events";
      string testMessage = $"{nameof(SendAndReceiveMessage_AreEqual)}:{DateTime.Now:dd.MM.yyyyTHH.mm.ss}";
      KafkaManager kafkaManager = new();
      await kafkaManager.SendAsync(TEST_TOPIC, testMessage);

      string kafkaMessage = await kafkaManager.ReadSingleMessageAsync(TEST_TOPIC);

      Assert.AreEqual(testMessage, kafkaMessage);
    }
  }
}