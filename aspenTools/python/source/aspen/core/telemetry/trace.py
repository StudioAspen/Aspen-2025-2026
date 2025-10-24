import functools
from opentelemetry import trace

def get_blender_tracer():
    """Gets the tracer for blender"""
    tracer = trace.get_tracer('aspen')

    return tracer

def trace_blender_function(span_name: str = None):
    """ Decorator for tracing blender functions.

    Args:
        span_name (str, optional): the name for the span. Defaults to None.

    Returns:
        Wrapped function
    """
    def decorator(func):
        @functools.wraps(func)
        def wrapper(*args, **kwargs):
            # Set span name to module and func name if not provided
            name = span_name or f"{func.__module__}.{func.__name__}"
            with get_blender_tracer().start_as_current_span(name) as span:
                # Add function meta data
                span.set_attribute("function.name", func.__name__)
                span.set_attribute("function.module", func.__module__)
                span.set_attribute("type", 'blender-function')

                try:
                    result = func(*args, **kwargs)
                    span.set_attribute("status", "success")
                    return result
                except Exception as ex:
                    span.set_attribute("status", "error")
                    span.record_exception(ex)
                    raise

        return wrapper

    return decorator

def trace_blender_operator(span_name: str = None):
    """ Decorator for tracing blender functions.

    Args:
        span_name (str, optional): the name for the span. Defaults to None.

    Returns:
        Wrapped function
    """
    def decorator(func):
        @functools.wraps(func)
        def wrapper(self, context):
            # Set span name to module and func name if not provided
            name = span_name or f"{func.__module__}.{self.__class__.__name__}"

            with get_blender_tracer().start_as_current_span(name) as span:
                # Add function meta data
                span.set_attribute("function.module", func.__module__)
                span.set_attribute("type", 'blender-operator')

                try:
                    result = func(self, context)
                    span.set_attribute("status", "success")
                    return result
                except Exception as ex:
                    span.set_attribute("status", "error")
                    span.record_exception(ex)
                    raise

        return wrapper

    return decorator