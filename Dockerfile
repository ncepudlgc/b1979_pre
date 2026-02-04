# Unity C# development environment
FROM mcr.microsoft.com/dotnet/sdk:8.0-jammy

# Install dependencies for Unity development
RUN apt-get update && apt-get install -y \
    git \
    curl \
    vim \
    nano \
    build-essential \
    && rm -rf /var/lib/apt/lists/*

# Set working directory
WORKDIR /workspace

# Copy project files
COPY . .

# Default to bash shell
CMD ["/bin/bash"]