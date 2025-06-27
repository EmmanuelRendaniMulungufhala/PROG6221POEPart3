using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CybersecurityChatbot
{
    public class Class1
    {
        private readonly Dictionary<string, List<string>> responses = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, List<string>> worryTips = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);
        private readonly List<string> pastQuestions = new List<string>();
        private readonly List<string> userInterests = new List<string>();
        private readonly Dictionary<string, int> interestTipIndexes = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        private string userName = null;
        private bool waitingForUserName = true;
        private bool askedWhy = false;
        private readonly Random random = new Random();

        private readonly Dictionary<string, Func<string>> menuOptions;

        private static readonly string[] fallbackResponses = new[]
        {
            "That's an interesting question! Let me think about it...",
            "Hmm, I don’t have an exact answer, but it sounds important.",
            "I’m always learning—thanks for the question!",
            "Not sure I know that, but let’s explore something related."
        };

        public Class1()
        {
            InitializeResponses();
            InitializeWorryTips();

            menuOptions = new Dictionary<string, Func<string>>(StringComparer.OrdinalIgnoreCase)
            {
                { "how are you", RespondHowAreYou },
                { "how are", RespondHowAreYou },
                { "purpose", RespondPurpose },
                { "what's your purpose", RespondPurpose },
                { "what can i ask", RespondMenu },
                { "what can i ask you about", RespondWhatCanIAskYouAbout },
                { "menu", RespondMenu },
                { "help", RespondMenu },
                { "okay", RespondMenu },
                { "alright", RespondMenu },
                { "sure", RespondMenu },
                { "phishing", () => GetNextTip("phishing") },
                { "password", () => GetNextTip("password") },
                { "passwords", () => GetNextTip("password") },
                { "scam", () => GetNextTip("scam") },
                { "scams", () => GetNextTip("scam") },
                { "malware", () => GetNextTip("malware") },
                { "hacking", () => GetNextTip("hacking") },
                { "hack", () => GetNextTip("hacking") },
                { "privacy", () => GetNextTip("privacy") },
                { "safe browsing", () => GetNextTip("safe browsing") },
                { "safe", () => GetNextTip("safe browsing") },
                { "two-factor authentication", () => GetNextTip("two-factor authentication") },
                { "2fa", () => GetNextTip("two-factor authentication") },
                { "encryption", () => GetNextTip("encryption") },
                { "encrypt", () => GetNextTip("encryption") }
            };
        }

        private void InitializeResponses()
        {
            responses["phishing"] = new List<string>
            {
                "Phishing is when attackers trick you into giving personal information. Never click on suspicious links!",
                "Always check the sender's email address. If it looks strange, it might be phishing.",
                "Look out for urgent language like 'Act now!' – it's often a phishing trick."
            };

            responses["password"] = new List<string>
            {
                "Use a mix of upper and lower case letters, numbers, and symbols in your password.",
                "Never reuse passwords across different sites.",
                "Consider using a password manager to store and generate secure passwords."
            };

            responses["scam"] = new List<string>
            {
                "Online scams often promise rewards. If it sounds too good to be true, it probably is.",
                "Avoid downloading attachments from unknown sources.",
                "Don’t share personal details over social media or unknown websites."
            };

            responses["malware"] = new List<string>
            {
                "Malware includes viruses, ransomware, and spyware designed to harm your device or steal data.",
                "Use antivirus software and keep it updated regularly.",
                "Avoid clicking on unknown email attachments or pop-ups."
            };

            responses["hacking"] = new List<string>
            {
                "Hacking is unauthorized access to systems. Use strong passwords and keep your software updated.",
                "Enable two-factor authentication to make hacking much harder.",
                "Avoid public Wi-Fi for sensitive activities to prevent hacking attempts."
            };

            responses["privacy"] = new List<string>
            {
                "Privacy means controlling who sees your information. Review app permissions regularly.",
                "Use encrypted messaging apps to protect your conversations.",
                "Be cautious about what you share on social media."
            };

            responses["safe browsing"] = new List<string>
            {
                "Always check for HTTPS in your browser to secure your connection.",
                "Avoid suspicious websites and pop-up heavy pages.",
                "Use browser extensions that block trackers and malicious scripts."
            };

            responses["two-factor authentication"] = new List<string>
            {
                "Two-factor authentication (2FA) adds a second layer of protection to your accounts.",
                "Use 2FA apps like Google Authenticator instead of SMS when possible.",
                "Even if someone gets your password, 2FA helps stop them from accessing your account."
            };

            responses["encryption"] = new List<string>
            {
                "Encryption secures your data by converting it into unreadable code unless you have the key.",
                "Use apps and services that offer end-to-end encryption, especially for messaging.",
                "Always encrypt sensitive files and backups to prevent unauthorized access."
            };
        }

        private void InitializeWorryTips()
        {
            worryTips["phishing"] = new List<string>
            {
                "Always check links before clicking – hover to see the real URL.",
                "Use email filters to reduce spam and phishing attempts.",
                "If in doubt, contact the sender via another method."
            };

            worryTips["password"] = new List<string>
            {
                "Avoid using the same password across multiple accounts.",
                "Change your passwords regularly, especially if there's been a data breach.",
                "Use two-factor authentication for an extra layer of protection."
            };

            worryTips["scam"] = new List<string>
            {
                "Research any offer before acting.",
                "Don't trust caller ID – scammers can spoof numbers.",
                "Never pay with gift cards or wire transfers."
            };

            worryTips["hacking"] = new List<string>
            {
                "Use two-factor authentication on all major accounts.",
                "Keep your OS and apps updated to patch vulnerabilities.",
                "Avoid using public Wi-Fi for sensitive activities."
            };

            worryTips["malware"] = new List<string>
            {
                "Download apps only from trusted sources.",
                "Use antivirus software and scan regularly.",
                "Don’t click unknown email attachments or pop-ups."
            };

            worryTips["privacy"] = new List<string>
            {
                "Use privacy settings on social media.",
                "Avoid sharing sensitive data on public platforms.",
                "Use encrypted messaging apps for personal communication."
            };

            worryTips["safe browsing"] = new List<string>
            {
                "Always check for HTTPS in your browser.",
                "Avoid sketchy websites or pop-up-heavy pages.",
                "Use browser extensions that block tracking scripts."
            };

            worryTips["two-factor authentication"] = new List<string>
            {
                "It’s okay if 2FA feels technical — start with one account, like your email.",
                "Try using an authenticator app; it's safer than SMS.",
                "Ask a trusted friend or colleague to help set up your first 2FA method."
            };

            worryTips["encryption"] = new List<string>
            {
                "Focus on using encrypted apps like Signal or ProtonMail for sensitive messages.",
                "You don’t need to understand all the math — just enable encryption features when offered.",
                "Start with encrypting your Wi-Fi router and device backups."
            };
        }

        public string GetBotResponse(string input)
        {
            if (waitingForUserName)
            {
                if (string.IsNullOrWhiteSpace(input))
                    return "What's my name?";

                string lower = input.ToLower().Trim();
                string name = null;

                if (lower.StartsWith("my name is "))
                    name = input.Substring(11).Trim();
                else if (lower.StartsWith("i am called "))
                    name = input.Substring(12).Trim();
                else
                    name = input.Trim();

                userName = string.IsNullOrWhiteSpace(name) ? null : name;
                if (userName == null)
                    return "Please tell me your name.";

                waitingForUserName = false;
                return $"Nice to meet you, {userName}! Here's what you can ask me:\n\n{GetMenuText()}";
            }

            input = input.Trim();
            pastQuestions.Add(input);
            string inputLower = input.ToLower();

            if (inputLower.Contains("remind me") || inputLower.Contains("what am i interested in"))
                return RespondRemind();

            if (inputLower.Contains("interested in"))
            {
                int idx = inputLower.IndexOf("interested in");
                if (idx >= 0 && idx + 13 < input.Length)
                {
                    string interest = input.Substring(idx + 13).Trim();
                    if (!string.IsNullOrWhiteSpace(interest))
                    {
                        if (!userInterests.Exists(i => i.Equals(interest, StringComparison.OrdinalIgnoreCase)))
                        {
                            userInterests.Add(interest);
                            interestTipIndexes[interest] = 0;
                            return $"Great, {userName}! I’ll remember that you're interested in {interest}.\n\n{GetMenuText()}";
                        }
                        else
                        {
                            return $"You already told me you're interested in {interest}, {userName}.\n\n{GetMenuText()}";
                        }
                    }
                    return $"Please tell me what you're interested in, {userName}.\n\n{GetMenuText()}";
                }
            }

            if (ContainsAny(inputLower, new[] { "worried", "sad", "concerned", "anxious" }))
            {
                var topicsInMessage = new List<string>();

                foreach (var topic in worryTips.Keys)
                {
                    if (inputLower.Contains(topic.ToLower()))
                        topicsInMessage.Add(topic);
                }

                foreach (var interest in userInterests)
                {
                    if (worryTips.ContainsKey(interest) && !topicsInMessage.Contains(interest, StringComparer.OrdinalIgnoreCase))
                        topicsInMessage.Add(interest);
                }

                if (topicsInMessage.Count == 0)
                    return $"I hear you, {userName}. It's okay to feel that way. Try asking about something specific like phishing, passwords, or scams.\n\n{GetMenuText()}";

                var sb = new StringBuilder();
                sb.Append($"I understand your feelings, {userName}. Here's some advice based on your concerns:\n\n");

                foreach (var topic in topicsInMessage)
                {
                    sb.Append($"🔹 {topic}: {GetNextWorryTip(topic)}\n");
                }

                sb.Append($"\n{GetMenuText()}");
                return sb.ToString();
            }

            if (ContainsAny(inputLower, new[] { "frustrated", "confused", "overwhelmed" }))
            {
                askedWhy = true;
                return $"It’s okay to feel that way, {userName}. Can you tell me what’s overwhelming you?\n\n{GetMenuText()}";
            }

            if (askedWhy)
            {
                askedWhy = false;
                return $"Thanks for explaining, {userName}. Awareness is the first step to protection.\n\n{GetMenuText()}";
            }

            if (inputLower.Contains("hi") || inputLower.Contains("hello"))
                return $"Hi {userName}! Ask me anything about staying safe online.\n\n{GetMenuText()}";

            foreach (var pair in menuOptions)
            {
                if (string.Equals(inputLower, pair.Key.ToLower(), StringComparison.OrdinalIgnoreCase))
                    return pair.Value.Invoke();
            }

            return GetFallbackResponse();
        }

        private bool ContainsAny(string input, string[] keywords)
        {
            foreach (var keyword in keywords)
            {
                if (input.Contains(keyword))
                    return true;
            }
            return false;
        }

        private string RespondHowAreYou() => "I'm just a cybersecurity bot here to help with your questions.";
        private string RespondPurpose() => "My purpose is to help you stay informed and protected from online threats.";

        private string RespondWhatCanIAskYouAbout() =>
            "You can ask me about a variety of cybersecurity topics, such as:\n" +
            "- Phishing\n- Passwords\n- Scams\n- Malware\n- Hacking\n- Privacy\n- Safe browsing\n" +
            "- Two-factor authentication\n- Encryption\n\n" +
            "I can also help you stay calm, provide tips when you're worried, and remember your interests.";

        private string RespondMenu() => GetMenuText();

        private string RespondRemind()
        {
            if (userInterests.Count == 0)
                return $"You haven’t told me what you're interested in yet, {userName}. Try saying: I'm interested in phishing.";

            var sb = new StringBuilder();
            sb.Append($"You're interested in: {string.Join(", ", userInterests)}, {userName}. Here's something for each:\n");

            foreach (var topic in userInterests)
                sb.Append($"🔹 {topic}: {GetNextTip(topic)}\n");

            return sb.ToString();
        }

        private string GetNextTip(string topic)
        {
            if (!responses.ContainsKey(topic) || responses[topic].Count == 0)
                return $"Sorry, I don't have tips for {topic}.";

            if (!interestTipIndexes.ContainsKey(topic))
                interestTipIndexes[topic] = 0;

            var tips = responses[topic];
            int idx = interestTipIndexes[topic];
            string tip = tips[idx];
            interestTipIndexes[topic] = (idx + 1) % tips.Count;

            return tip;
        }

        private string GetNextWorryTip(string topic)
        {
            if (!worryTips.ContainsKey(topic) || worryTips[topic].Count == 0)
                return "Stay calm and practice good cybersecurity habits.";

            string key = "worry_" + topic;
            if (!interestTipIndexes.ContainsKey(key))
                interestTipIndexes[key] = 0;

            var tips = worryTips[topic];
            int idx = interestTipIndexes[key];
            string tip = tips[idx];
            interestTipIndexes[key] = (idx + 1) % tips.Count;

            return tip;
        }

        private string GetMenuText() =>
            "🔐 You can ask me about:\n" +
            "- How are you\n" +
            "- What's your purpose\n" +
            "- What can I ask you about\n" +
            "- Phishing\n" +
            "- Passwords\n" +
            "- Scams\n" +
            "- Malware\n" +
            "- Hacking\n" +
            "- Privacy\n" +
            "- Safe browsing\n" +
            "- Two-factor authentication\n" +
            "- Encryption\n" +
            "Or say: I'm interested in [topic]";

        private string GetFallbackResponse() =>
            fallbackResponses[random.Next(fallbackResponses.Length)];
    }
}
