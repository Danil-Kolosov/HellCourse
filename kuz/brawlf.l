%{
#include <stdio.h>
#include <stdlib.h>
#include <string.h>
#include "brawlf.tab.h"

void yyerror(const char *s);
//int lineno = 1;
%}

%option noyywrap          
%option case-insensitive
%option yylineno

DIGIT   [0-9]
ID      [a-zA-Z_][a-zA-Z0-9_]*
WS      [ \t\r]

%%

[\n]            { }

":primo:"       { return PRIMO; }
":gem:"         { return GEM; }
":note:"        { return NOTE; }
":brain:"       { return BRAIN; }
":sparkles:"    { return SPARKLES; }
"🔢"            { return PRIMO; }
"💎"            { return GEM; }
"🎵"            { return NOTE; }
"🧠"            { return BRAIN; }
"✨"            { return SPARKLES; }

":boom:"        { return BOOM; }
":shield:"      { return SHIELD; }
":star:"        { return STAR; }
":droplet:"     { return DROPLET; }
":zap:"         { return ZAP; }
":fire:"        { return FIRE; }
":snowflake:"   { return SNOWFLAKE; }
":sun:"         { return SUN; }
":moon:"        { return MOON; }
":bulb:"        { return BULB; }
":link:"        { return LINK; }
":no_entry:"    { return NO_ENTRY; }
"💥"            { return BOOM; }
"🛡️"           { return SHIELD; }
"⭐"            { return STAR; }
"💧"            { return DROPLET; }
"⚡"            { return ZAP; }
"🔥"            { return FIRE; }
"❄️"           { return SNOWFLAKE; }
"☀️"           { return SUN; }
"🌙"           { return MOON; }
"💡"            { return BULB; }
"🔗"            { return LINK; }
"⛔"            { return NO_ENTRY; }

":crown:"       { return CROWN; }
":cyclone:"     { return CYCLONE; }
":infinity:"    { return INFINITY; }
":dart:"        { return DART; }
":skull:"       { return SKULL; }
":collision:"   { return COLLISION; }
":loop:"        { return LOOP; }
"👑"            { return CROWN; }
"🌪️"           { return CYCLONE; }
"♾️"           { return INFINITY; }
"🎯"            { return DART; }
"💀"            { return SKULL; }
"🛑"            { return COLLISION; }
"➿"            { return LOOP; }
"🔙"            { return BACK; }
"👉"            { return IN; }

":in:"          { return IN; }
[.][.]          { return DOTDOT; }
[.][.]<         { return DOTDOTLT; }
":step:"        { return STEP; }
"🎬"            { return STEP; }


":locomotive:"       { return LOCOMOTIVE; }
":steam_locomotive:" { return STEAM_LOCOMOTIVE; }
":railway_track:"    { return RAILWAY_TRACK; }
"🚂"            { return LOCOMOTIVE; }
"🚂🚂"         { return STEAM_LOCOMOTIVE; }
"🛤️"           { return RAILWAY_TRACK; }
":chak:"          { return CHAK; }
"📦"            { return CHAK; }


":back:"        { return BACK; }
":ghost:"       { return GHOST; }
"👻"            { return GHOST; }

":mega:"        { return MEGA; }
"📢"            { return MEGA; }
":ear:"         { return EAR; }
"👂"            { return EAR; }

":sin:"         { return SIN; }
":cos:"         { return COS; }
"📉"            { return SIN; }
"📈"            { return COS; }

":heart:"       { return HEART; }
":broken_heart:" { return BROKEN_HEART; }
":muscle:"      { return MUSCLE; }
":bone:"        { return BONE; }
":triangular_ruler:" { return TRULER; }
"❤️"           { return HEART; }
"💔"           { return BROKEN_HEART; }
"💪"           { return MUSCLE; }
"🦴"           { return BONE; }
"☮"             { return TRULER; }




{DIGIT}+        { yylval.number = atoi(yytext); return NUMBER; }
{DIGIT}+"."{DIGIT}*  { yylval.fnumber = atof(yytext); return FLOAT; }
"true"          { yylval.number = 1; return TRUE; }
"false"         { yylval.number = 0; return FALSE; }
{ID}            { yylval.string = strdup(yytext); return IDENT; }
\"[^"]*\"       { yylval.string = strdup(yytext); return STRING; }
'(\\.|[^'\\])'  { yylval.string = strdup(yytext); return CHAR; }


{WS}            { /* ignore */ }
"//"[^\n]*      { /* ignore */ }


.               { 
    fprintf(stderr, "Lexical error: Invalid character '%s'\n", yytext); 
    return ERROR; 
}

%%