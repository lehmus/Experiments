# Encoding categorical variables
# http://pbpython.com/categorical-encoding.html

import pandas as pd
import numpy as np

dataset_url = "https://archive.ics.uci.edu/ml/machine-learning-databases/autos/imports-85.data"
dataset_file = "C:\\Users\\LauriLehman\\Documents\\Projects\\Experiments\\PythonExamples\\Assets\\imports-85.csv"

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


# print("Number of samples in the dataset: " + str(len(dataset)))

# print("Data types:")
# print(dataset.dtypes)

# print("First rows of the data set:")
# print(dataset[1:10])


# Filter categorical variables
# ===
categorical_dataset = dataset.select_dtypes(include=['object']).copy()

print("Rows with null values:")
print(categorical_dataset[categorical_dataset.isnull().any(axis=1)])

print("Values of column num_doors:")
print(categorical_dataset["num_doors"].value_counts())

# For the sake of simplicity, just fill in the value with the number 4 (since that is the most common value):
# ===
categorical_dataset = categorical_dataset.fillna({"num_doors": "four"})


# Case 1: Convert integer strings to integers
# ===
cleanup_nums = {"num_doors":     {"four": 4, "two": 2},
                "num_cylinders": {"four": 4, "six": 6, "five": 5, "eight": 8,
                                  "two": 2, "twelve": 12, "three":3
                                 }
               }
categorical_dataset.replace(cleanup_nums, inplace=True)


# Case 2: Label encoding (map strings to integers)

# One trick you can use in pandas is to convert a column to a category, then use those category values for your label encoding:
categorical_dataset["body_style"] = categorical_dataset["body_style"].astype('category')
# Then you can assign the encoded variable to a new column using the cat.codes accessor:
categorical_dataset["body_style_cat"] = categorical_dataset["body_style"].cat.codes
#  The nice aspect of this approach is that you get the benefits of pandas categories (compact data size, ability to order, plotting support) but can easily be converted to numeric values for further analysis.

print("Label encoded body_style")
print(categorical_dataset.head())


# Case 3: One-hot encoding

one_hot_dataset = pd.get_dummies(dataset, columns=["body_style", "drive_wheels"], prefix=["body", "drive"])
print("one-hot encoded body_style and drive_wheels")
print(one_hot_dataset.head())
