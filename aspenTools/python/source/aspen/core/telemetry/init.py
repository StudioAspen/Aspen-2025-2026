import logging
from opentelemetry import trace
from opentelemetry.sdk.resources import Resource
from opentelemetry.exporter.otlp.proto.grpc._log_exporter import OTLPLogExporter

from opentelemetry.sdk._logs import LoggerProvider, LoggingHandler
from opentelemetry.sdk._logs.export import BatchLogRecordProcessor
from opentelemetry._logs import set_logger_provider

from opentelemetry.sdk.trace import TracerProvider
from opentelemetry.exporter.otlp.proto.grpc.trace_exporter import OTLPSpanExporter
from opentelemetry.sdk.trace.export import BatchSpanProcessor

END_POINT = '135.180.232.175:4317'

def initialize(service_name: str):
    """Initialize the telemetry providers and handlers."""

    # Create resource for initializing both logger and tracer provider
    resource = Resource.create({'service.name': f'aspen-{service_name}'})

    # Set up logger
    logger_provider = LoggerProvider(resource=resource)
    set_logger_provider(logger_provider)

    log_exporter = OTLPLogExporter(endpoint=END_POINT, insecure=True)
    logger_provider.add_log_record_processor(BatchLogRecordProcessor(log_exporter))

    handler = LoggingHandler(level=logging.INFO, logger_provider=logger_provider)

    logging.getLogger('aspen').addHandler(handler)
    logging.getLogger('aspen').setLevel(logging.INFO)

    # Set up tracer
    span_exporter = OTLPSpanExporter(endpoint=END_POINT, insecure=True)

    tracer_provider = TracerProvider(resource=resource)
    tracer_provider.add_span_processor(BatchSpanProcessor(span_exporter))
    trace.set_tracer_provider(tracer_provider)