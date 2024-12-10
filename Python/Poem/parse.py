import os
import re
import pandas as pd

class Item:
    def __init__(self, title=None, content=None, author=None, dynasty=None, scope=None):
        self.title = title
        self.content = content
        self.author = author
        self.dynasty = dynasty
        self.scope = scope




def get_files_with_extension(extension):
    files = []
    for root, dirs, file_names in os.walk('.'):
        for file_name in file_names:
            if file_name.endswith(extension):
                files.append(os.path.join(root, file_name))
    return files

def get_filename_without_extension(file_path):
    return os.path.splitext(os.path.basename(file_path))[0]

def combine_lists_remove_duplicates(list1, list2):
    combined_list = list1.copy()
    titles = {item.title for item in list1}
    
    for item in list2:
        if item.title not in titles :
            combined_list.append(item)
            titles.add(item.title)
    
    return combined_list

def extract_title(input):
    if(input.count("[") != 0):
        match = re.search(r'\[(.*?)\]', input)
        if match:
            return match.group(1)
    if(input.count(".") != 0):
        match = re.search(r'\d+\.(.*)', input)
        if match:
            return match.group(1)
    return input

def extract_content(input):
    match = re.search(r'```(.*?)```', input, re.DOTALL)
    if match:
        return match.group(1).strip()
    return None

def extract_author_and_dynasty(input):
    author, dynasty = None, None
    author_pattern = r"#### (.*?)\n"
    author_dynasty_pattern = r"#### (.*?) - (.*?)\n"
    author_dynasty_pattern2 = r"#### (.*?)：(.*?)\n"
    match = re.search(author_pattern,input)
    if match:
        if match.group(1).count("-") != 0:
            author_match = re.search(author_dynasty_pattern, input)
            if author_match:
                dynasty, author = author_match.groups()
        elif match.group(1).count("：") != 0:
            author_match = re.search(author_dynasty_pattern2, input)
            if author_match:
                dynasty, author = author_match.groups()
        else:
            author = match.group(1)

    if(dynasty is not None and dynasty!='五代' and dynasty.endswith('代')):
        dynasty = dynasty[:-1]

    return author, dynasty

def extract_poems(file_path):
    # Regex patterns for parsing
    title_pattern = r"\n###? (.*?)\n"

    # Open and read the markdown file
    with open(file_path, "r", encoding="utf-8") as file:
        content = file.read()

    # Initialize a list to store poems
    poems = []

    # Parse the markdown content
    titles = re.findall(title_pattern, content)
    contents = re.split(title_pattern, content)[2::2]  # Split based on titles
    for i in range(len(titles)):
        # Try to match author and dynasty
        author, dynasty = extract_author_and_dynasty(contents[i])

        # Extract poem content
        poem_content = extract_content(contents[i])

        # Append to poems list
        if(titles[i] is not None and poem_content is not None and len(poem_content) > 0):
            poems.append(Item(
                title=extract_title(titles[i]),
                content=poem_content,
                author=author,
                dynasty=dynasty,
                scope=get_filename_without_extension(file_path)
            ))

    return poems

files = get_files_with_extension(".md")

full_poems = []

for file_path in files:
    poems = extract_poems(file_path)
    full_poems = combine_lists_remove_duplicates(full_poems, poems)


df = pd.DataFrame([item.__dict__ for item in full_poems])
df.to_csv('combined_list.csv', index=False, encoding='utf-8-sig')
df.to_json('combined_list.json', orient='records', force_ascii=False)