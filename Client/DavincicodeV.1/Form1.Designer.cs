namespace DavincicodeV._1
{
    partial class Form1
    {
        /// <summary>
        /// 필수 디자이너 변수입니다.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 사용 중인 모든 리소스를 정리합니다.
        /// </summary>
        /// <param name="disposing">관리되는 리소스를 삭제해야 하면 true이고, 그렇지 않으면 false입니다.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form 디자이너에서 생성한 코드

        /// <summary>
        /// 디자이너 지원에 필요한 메서드입니다. 
        /// 이 메서드의 내용을 코드 편집기로 수정하지 마세요.
        /// </summary>
        private void InitializeComponent()
        {
            this.flowOpponentCards = new System.Windows.Forms.FlowLayoutPanel();
            this.flowPlayerCards = new System.Windows.Forms.FlowLayoutPanel();
            this.btnConnectClient = new System.Windows.Forms.Button();
            this.txtLog = new System.Windows.Forms.TextBox();
            this.txtServerIP = new System.Windows.Forms.TextBox();
            this.cmbColor = new System.Windows.Forms.ComboBox();
            this.cmbNumber = new System.Windows.Forms.ComboBox();
            this.cmbIndex = new System.Windows.Forms.ComboBox();
            this.btnGuess = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // flowOpponentCards
            // 
            this.flowOpponentCards.Location = new System.Drawing.Point(190, 59);
            this.flowOpponentCards.Name = "flowOpponentCards";
            this.flowOpponentCards.Size = new System.Drawing.Size(930, 102);
            this.flowOpponentCards.TabIndex = 0;
            // 
            // flowPlayerCards
            // 
            this.flowPlayerCards.Location = new System.Drawing.Point(190, 301);
            this.flowPlayerCards.Name = "flowPlayerCards";
            this.flowPlayerCards.Size = new System.Drawing.Size(930, 102);
            this.flowPlayerCards.TabIndex = 0;
            // 
            // btnConnectClient
            // 
            this.btnConnectClient.Location = new System.Drawing.Point(717, 411);
            this.btnConnectClient.Name = "btnConnectClient";
            this.btnConnectClient.Size = new System.Drawing.Size(120, 48);
            this.btnConnectClient.TabIndex = 1;
            this.btnConnectClient.Text = "서버 접속";
            this.btnConnectClient.UseVisualStyleBackColor = true;
            this.btnConnectClient.Click += new System.EventHandler(this.btnConnectClient_Click);
            // 
            // txtLog
            // 
            this.txtLog.Dock = System.Windows.Forms.DockStyle.Left;
            this.txtLog.Location = new System.Drawing.Point(0, 0);
            this.txtLog.Multiline = true;
            this.txtLog.Name = "txtLog";
            this.txtLog.ReadOnly = true;
            this.txtLog.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.txtLog.Size = new System.Drawing.Size(184, 499);
            this.txtLog.TabIndex = 2;
            // 
            // txtServerIP
            // 
            this.txtServerIP.Location = new System.Drawing.Point(389, 438);
            this.txtServerIP.Name = "txtServerIP";
            this.txtServerIP.Size = new System.Drawing.Size(82, 21);
            this.txtServerIP.TabIndex = 2;
            // 
            // cmbColor
            // 
            this.cmbColor.FormattingEnabled = true;
            this.cmbColor.Location = new System.Drawing.Point(190, 197);
            this.cmbColor.Name = "cmbColor";
            this.cmbColor.Size = new System.Drawing.Size(116, 20);
            this.cmbColor.TabIndex = 3;
            // 
            // cmbNumber
            // 
            this.cmbNumber.FormattingEnabled = true;
            this.cmbNumber.Location = new System.Drawing.Point(322, 197);
            this.cmbNumber.Name = "cmbNumber";
            this.cmbNumber.Size = new System.Drawing.Size(116, 20);
            this.cmbNumber.TabIndex = 3;
            // 
            // cmbIndex
            // 
            this.cmbIndex.FormattingEnabled = true;
            this.cmbIndex.Location = new System.Drawing.Point(456, 197);
            this.cmbIndex.Name = "cmbIndex";
            this.cmbIndex.Size = new System.Drawing.Size(116, 20);
            this.cmbIndex.TabIndex = 3;
            // 
            // btnGuess
            // 
            this.btnGuess.Location = new System.Drawing.Point(601, 197);
            this.btnGuess.Name = "btnGuess";
            this.btnGuess.Size = new System.Drawing.Size(115, 20);
            this.btnGuess.TabIndex = 4;
            this.btnGuess.Text = "추리하기";
            this.btnGuess.UseVisualStyleBackColor = true;
            this.btnGuess.Click += new System.EventHandler(this.btnGuess_Click);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1172, 499);
            this.Controls.Add(this.btnGuess);
            this.Controls.Add(this.cmbIndex);
            this.Controls.Add(this.cmbNumber);
            this.Controls.Add(this.cmbColor);
            this.Controls.Add(this.txtServerIP);
            this.Controls.Add(this.txtLog);
            this.Controls.Add(this.btnConnectClient);
            this.Controls.Add(this.flowPlayerCards);
            this.Controls.Add(this.flowOpponentCards);
            this.Name = "Form1";
            this.Text = "Form1";
            this.Load += new System.EventHandler(this.Form1_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.FlowLayoutPanel flowOpponentCards;
        private System.Windows.Forms.FlowLayoutPanel flowPlayerCards;
        private System.Windows.Forms.Button btnConnectClient;
        private System.Windows.Forms.TextBox txtLog;
        private System.Windows.Forms.TextBox txtServerIP;
        private System.Windows.Forms.ComboBox cmbColor;
        private System.Windows.Forms.ComboBox cmbNumber;
        private System.Windows.Forms.ComboBox cmbIndex;
        private System.Windows.Forms.Button btnGuess;
    }
}

