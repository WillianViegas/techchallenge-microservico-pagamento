resource "aws_iam_role" "ws_iam_role_eks" {
  name = "my-cluster-eks-pagamento"

  assume_role_policy = <<POLICY
{
  "Version": "2012-10-17",
  "Statement": [
    {
      "Effect": "Allow",
      "Principal": {
        "Service": "eks.amazonaws.com"
      },
      "Action": "sts:AssumeRole"
    }
  ]
}
POLICY
}

resource "aws_iam_role_policy_attachment" "ws_iam_role_eks-AmazonEKSClusterPolicy" {
  policy_arn = "arn:aws:iam::aws:policy/AmazonEKSClusterPolicy"
  role       = aws_iam_role.ws_iam_role_eks.name
}

resource "aws_eks_cluster" "my_cluster_eks_pagamento" {
  name     = "my-cluster-eks-pagamento"
  role_arn = aws_iam_role.ws_iam_role_eks.arn

  vpc_config {
    subnet_ids = [
      aws_subnet.private-us-east-1a.id,
      aws_subnet.private-us-east-1b.id,
      aws_subnet.public-us-east-1a.id,
      aws_subnet.public-us-east-1b.id
    ]
  }

  depends_on = [aws_iam_role_policy_attachment.ws_iam_role_eks-AmazonEKSClusterPolicy]
}
