import pandas as pd
import numpy as np

dataset_url = "https://archive.ics.uci.edu/ml/machine-learning-databases/autos/imports-85.data"
dataset_file = "C:\\Users\\lauri\\Documents\\Experiments\\TestiNotebook\\Assets\\imports-85.csv"

# Define the headers since the data does not have any
headers = ["symboling", "normalized_losses", "make", "fuel_type", "aspiration",
           "num_doors", "body_style", "drive_wheels", "engine_location",
           "wheel_base", "length", "width", "height", "curb_weight",
           "engine_type", "num_cylinders", "engine_size", "fuel_system",
           "bore", "stroke", "compression_ratio", "horsepower", "peak_rpm",
           "city_mpg", "highway_mpg", "price"]

dataset = pd.DataFrame()

# Read in the CSV file and convert "?" to NaN
try:
    dataset = pd.read_csv(dataset_file)
    print("Using dataset from file " + dataset_file)
except FileNotFoundError:
    print("The data source file could not be found: " + dataset_file)
    print("Fetching data from " + dataset_url)
    dataset = pd.read_csv(dataset_url, header=None, names=headers, na_values="?")
    print("Saving the source data frame to file: " + dataset_file)
    dataset.to_csv(path_or_buf=dataset_file)


print("Number of samples in the dataset: " + str(len(dataset)))

# print("Data types:")
# print(dataset.dtypes)

# print("First rows of the data set:")
## print(dataset[1:10])

# objectdataset = dataset.select_dtypes(include=['object']).copy()
# objectdataset[objectdataset.isnull().any(axis=1)]
