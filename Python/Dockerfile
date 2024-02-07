FROM python:3.12.1

# Copy everything
COPY . ./

# RUN python -m ensurepip
RUN pip install  plotly
# RUN pip install matplotlib
# RUN pip install numpy

WORKDIR  /ColumnActivityDiagram

# Run hello.py when the container launches
ENTRYPOINT ["python", "draw_figure.py"]

# Pass arguments
CMD ["-fn", "sampleOne.txt", "-gn", "A2024123", "-mc", "19", "-ht", "8", "-yt", "yaxis", "-xt", "xaxis", "-min", "50", "-max", "4000", "-st", "'single column'", "-fign", "CortialColumn", "-a"]
