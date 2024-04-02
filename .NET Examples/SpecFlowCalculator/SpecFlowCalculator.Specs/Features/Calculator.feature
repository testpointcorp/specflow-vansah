Feature: Calculator

@Add
Scenario: Add two number
	Given the first number is 50
	And the second number is 70
	When the two numbers are added
	When the Test Case key is Test-C8 And the Issue Key is Test-2
	Then the result should be 120

@Subtract
Scenario: Subtract two numbers
	Given the first number is 50
	And the second number is 25
	When the two numbers are subtracted
	When the Test Case key is Test-C9 And the Test Folder ID is b97fe80b-0b6a-11ee-8e52-5658ef8eadd5
	Then the result should be 25