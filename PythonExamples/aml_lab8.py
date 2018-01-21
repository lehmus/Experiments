def azureml_main(dataframe1 = None):

    import pandas as pd
    # Only use the columns of interest and restrict the rows to those in the US
    # census_data = dataframe1[['age', 'fnlwgt', 'education-num', 'sex', 'native-country', 'income']]

    # census_data = census_data.loc[census_data['native-country'] == "United-States"]

    # del census_data['native-country']

    # factorize the sex column

    # census_data['sex'], sex_index = pd.factorize(census_data['sex'])

    print ('valmis')
    # return census_data

azureml_main()
