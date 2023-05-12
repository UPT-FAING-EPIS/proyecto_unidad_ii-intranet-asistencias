import pika
import sys
import json
import datetime

params = pika.ConnectionParameters(host='localhost')
connection = pika.BlockingConnection(params)
channel = connection.channel()

queue_name = 'logs'
channel.queue_declare(queue=queue_name)

log_entry = {
    "Timestamp": datetime.datetime.now().isoformat(),
    "Level": "INFO",
    "Message": "Esto es una prueba de fuego"
}
channel.basic_publish(exchange='', routing_key='logs', body=json.dumps(log_entry))
print(" [x] Sent log message")


connection.close()
