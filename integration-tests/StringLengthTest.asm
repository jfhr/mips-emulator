# Szenario: Get length of zero-terminated string
# $a0 contains the string adress
# $v0 will contain the string length

# $t0 - adress of the current char
# $t1 - contents of the current char
# $t2 - length of the string so far

.globl strlen

strlen:
move $t0,$a0
li $t2,0

loop:
lb $t1,($t0)
beqz $t1,end
addi $t0,$t0,1
addi $t2,$t2,1
b loop

end:
move $v0,$t2
